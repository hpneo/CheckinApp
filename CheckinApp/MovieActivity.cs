using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;


using Android.Gms.Maps;
using Android.Gms.Maps.Model;

using CheckinShared.Models;
using CheckinShared.Services;


namespace CheckinAppAndroid
{
	[Activity (Label = "CheckinApp", Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.Light")]			
	public class MovieActivity : Activity
	{
		private CheckinShared.MovieDB movies;
		private CheckinShared.CheckinDB checkins;

		private GoogleMap _map;
		private MapFragment _mapFragment;
		private static readonly LatLng UPC = new LatLng(-12.103951800, -76.963278100);

		async protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			//Inicialización del mapa

			InitMapFragment ();

			movies = new CheckinShared.MovieDB ();
			checkins = new CheckinShared.CheckinDB ();

			int movieId = this.Intent.GetIntExtra ("movieId", 0);
			string mode = this.Intent.GetStringExtra ("mode");
			var movie = movies.Get (movieId);

			TMDB api = new TMDB ();
			if (movie.Overview == null) {
				JObject movieJSON = await api.Find (movie.ApiId) as JObject;
				movie.Overview = movieJSON ["overview"].ToString ();

				movies.Update (movie);
			}

			if (movie.Director == null) {
				JObject movieCreditsJSON = await api.GetCredits (movie.ApiId) as JObject;

				if (movieCreditsJSON ["crew"].Count () > 0) {
					movie.Director = movieCreditsJSON ["crew"] [0] ["name"].ToString ();
				}

				movie.Cast = "";

				for (var i = 0; i < movieCreditsJSON ["cast"].Count (); i++) {
					movie.Cast += movieCreditsJSON ["cast"] [i] ["name"].ToString () + "\n";
				}

				movies.Update (movie);
			}

			SetContentView (Resource.Layout.Movie);

			TextView textViewMovieTitle = FindViewById<TextView> (Resource.Id.textViewMovieTitle);
			TextView textViewMovieDescription = FindViewById<TextView> (Resource.Id.textViewMovieDescription);
			TextView textViewMovieDirector = FindViewById<TextView> (Resource.Id.textViewMovieDirector);
			TextView textViewMovieYear = FindViewById<TextView> (Resource.Id.textViewMovieYear);
			TextView textViewMovieCast = FindViewById<TextView> (Resource.Id.textViewMovieCast);
			ImageView imageViewMoviePoster = FindViewById<ImageView> (Resource.Id.imageViewMoviePoster);

			Button buttonCheckin = FindViewById<Button> (Resource.Id.buttonCheckin);
			Button buttonShareFacebook = FindViewById<Button> (Resource.Id.buttonShareFacebook);
			Button buttonShareTwitter = FindViewById<Button> (Resource.Id.buttonShareTwitter);

			if (mode == "info") {
				buttonCheckin.Visibility = ViewStates.Gone;
			}

			textViewMovieTitle.Text = movie.Title;
			textViewMovieYear.Text = movie.Year;
			textViewMovieDescription.Text = movie.Overview;
			if (movie.Director != null) {
				textViewMovieDirector.Text = movie.Director;
			} else {
				textViewMovieDirector.Visibility = ViewStates.Gone;
			}

			if (movie.Cast != null) {
				textViewMovieCast.Text = movie.Cast;
			} else {
				textViewMovieCast.Visibility = ViewStates.Gone;
			}

			if (movie.Poster != null) {
				imageViewMoviePoster.SetImageBitmap ((Android.Graphics.Bitmap)movie.Poster);
			} else {
				Koush.UrlImageViewHelper.SetUrlDrawable (imageViewMoviePoster, movie.PosterPath);
				movie.Poster = Koush.UrlImageViewHelper.GetCachedBitmap (movie.PosterPath);
			}

			var self = this;

			buttonCheckin.Click += (object sender, EventArgs e) => {
				Checkin checkin = new Checkin ();
				checkin.MovieId = movie.Id;
				checkin.CreatedAt = DateTime.UtcNow;

				checkins.Insert (checkin);

				Intent intent = new Intent ();
				intent.PutExtra ("checkinId", checkin.Id);
				intent.PutExtra ("movieId", movie.Id);

				SetResult (Result.Ok, intent);
				Finish ();
			};

			var sharedPreferences = GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);


			buttonShareFacebook.Click += async (object sender, EventArgs e) => {
				var facebookClient = new CheckinShared.Facebook ("1492339931014967", "7ae094df0f071a1972ed7c7354943f9a");
				facebookClient.UserToken = sharedPreferences.GetString ("Facebook:token", "");

				if (facebookClient.UserToken != "") {
					try {
						string result = await facebookClient.PublishFeed (new {
							message = "Estoy viendo " + movie.Title,
							link = "https://www.themoviedb.org/movie/" + movie.ApiId,
							picture = movie.PosterPath,
							name = movie.Title,
							caption = movie.Year,
							description = movie.Overview
						}) as string;

						Console.WriteLine("result: " + result);

						Toast.MakeText (self, "Película compartida en Facebook", ToastLength.Short).Show ();
					} catch (Exception ex) {
						Console.WriteLine("ex.StackTrace");
						Console.WriteLine(ex.StackTrace);
						Console.WriteLine("ex.StackTrace");
						Toast.MakeText (self, "Hubo un error al compartir la película: " + ex.Source, ToastLength.Long).Show ();
					}
				} else {
					Intent intent = new Intent (this, typeof(AuthActivity));
					intent.PutExtra ("authService", "Facebook");
					StartActivityForResult (intent, (int)RequestsConstants.AuthRequest);
				}
			};

			buttonShareTwitter.Click += async (object sender, EventArgs e) => {
				var twitterClient = new CheckinShared.Twitter ("IO0mSObd1KnbSOkZXBvGchomD", "JiCrmSCOp0AR2m0zIjoY8Cq1STTbcjEPupMdpOkEihmHViQ5Lh");
				twitterClient.UserToken = sharedPreferences.GetString ("Twitter:token", "");
				twitterClient.UserSecret = sharedPreferences.GetString ("Twitter:secret", "");

				if (twitterClient.UserToken != "" && twitterClient.UserSecret != "") {
					try {
						string result = await twitterClient.UpdateStatus (new {
							status = "Estoy viendo " + movie.Title + " (" + "https://www.themoviedb.org/movie/" + movie.ApiId + ")"
						}) as string;

						Console.WriteLine("result: " + result);

						Toast.MakeText (self, "Película compartida en Twitter", ToastLength.Short).Show ();
					} catch (Exception ex) {
						Console.WriteLine("ex.Message");
						Console.WriteLine(ex.Message);
						Console.WriteLine("ex.Message");
						Toast.MakeText (self, "Hubo un error al compartir la película: " + ex.Source, ToastLength.Long).Show ();
					}
				} else {
					Intent intent = new Intent (this, typeof(AuthActivity));
					intent.PutExtra ("authService", "Twitter");
					StartActivityForResult (intent, (int)RequestsConstants.AuthRequest);
				}
			};

			ActionBar.SetDisplayHomeAsUpEnabled (true);
			ActionBar.SetDisplayShowTitleEnabled (true);
			ActionBar.SetDisplayShowHomeEnabled (true);
			ActionBar.SetDisplayUseLogoEnabled (false);
			Window.SetTitle (movie.Title);
		}

		private void InitMapFragment()
		{
			_mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;
			if (_mapFragment == null)
			{
				GoogleMapOptions mapOptions = new GoogleMapOptions()
					.InvokeCamera(CameraPosition.FromLatLngZoom(new LatLng(-12.103951800, -76.963278100), 16))
					.InvokeScrollGesturesEnabled(true)
					.InvokeMapType(GoogleMap.MapTypeNormal)
					.InvokeZoomControlsEnabled(true)
					.InvokeCompassEnabled(true);
				//	-12.103951800, -76.963278100

				FragmentTransaction fragTx = FragmentManager.BeginTransaction();
				_mapFragment = MapFragment.NewInstance(mapOptions);
				fragTx.Add(Resource.Id.map, _mapFragment, "map");
				fragTx.Commit();
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			SetupMapIfNeeded();
		}

		private void SetupMapIfNeeded()
		{
			if (_map == null)
			{
				_map = _mapFragment.Map;
				if (_map != null)
				{
						_map = _mapFragment.Map;

						if (_map != null) {
							MarkerOptions markerOpt1 = new MarkerOptions();
							markerOpt1.SetPosition(UPC);
							markerOpt1.SetTitle("CheckinApp");
							_map.AddMarker(markerOpt1);
						}

				}
			}
		}


		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Android.Resource.Id.Home) {
				OnBackPressed ();
			}

			return base.OnOptionsItemSelected (item);
		}

		async protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			if (requestCode == (int)RequestsConstants.AuthRequest) {
				if (resultCode == Result.Ok) {
					string result;
					/*if (intent.GetStringExtra ("authService") == "Facebook") {
						var facebookClient = new CheckinShared.Facebook ("1492339931014967", "7ae094df0f071a1972ed7c7354943f9a");

						facebookClient.UserToken = intent.GetStringExtra ("token");
						result = await facebookClient.PublishFeed (new {
							message = "En CanchitApp!"
						}) as string;
					} else {
						var twitterClient = new CheckinShared.Twitter ("IO0mSObd1KnbSOkZXBvGchomD", "JiCrmSCOp0AR2m0zIjoY8Cq1STTbcjEPupMdpOkEihmHViQ5Lh");
						twitterClient.UserToken = intent.GetStringExtra ("token");
						twitterClient.UserSecret = intent.GetStringExtra ("secret");

						Console.WriteLine ("Token: " + twitterClient.UserToken);
						Console.WriteLine ("Secret: " + twitterClient.UserSecret);

						result = await twitterClient.UpdateStatus (new {
							status = "En CanchitApp!"
						}) as string;
					}
					Console.WriteLine (result);*/
				}
			}
		}

	
	}
}

