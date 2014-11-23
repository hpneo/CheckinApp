using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Locations;
using Android.Util;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;

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
	public class MovieActivity : Activity, ILocationListener
	{
		private CheckinShared.MovieDB movies;
		private CheckinShared.CheckinDB checkins;

		private GoogleMap map;
		private MapFragment mapFragment;
		private static readonly LatLng UPC = new LatLng (-12.103951800, -76.963278100);
		LocationManager locMgr;
		string mode;
		CheckinShared.Models.Movie movie;

		Android.Locations.Location currentLocation;

		async protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Movie);

			movies = new CheckinShared.MovieDB ();
			checkins = new CheckinShared.CheckinDB ();

			int movieId = this.Intent.GetIntExtra ("movieId", 0);
			mode = this.Intent.GetStringExtra ("mode");
			movie = movies.Get (movieId);

			TMDB api = new TMDB ();

			if (mode != "info") {
				InitMapFragment ();
			}

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

			movie.SaveToParse ();

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

				if (mapFragment != null && mapFragment.View != null) {
					mapFragment.View.Visibility = ViewStates.Gone;
				}
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
				checkin.UserId = AppHelper.GetCurrentUser(this).Id;
				checkin.CreatedAt = DateTime.UtcNow;

				if (currentLocation != null) {
					checkin.Latitude = currentLocation.Latitude;
					checkin.Longitude = currentLocation.Longitude;
				}

				checkins.Insert (checkin);

				checkin.SaveToParse();

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

						Console.WriteLine ("result: " + result);

						Toast.MakeText (self, "Película compartida en Facebook", ToastLength.Short).Show ();
					} catch (Exception ex) {
						Console.WriteLine ("ex.StackTrace");
						Console.WriteLine (ex.StackTrace);
						Console.WriteLine ("ex.StackTrace");
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

						Console.WriteLine ("result: " + result);

						Toast.MakeText (self, "Película compartida en Twitter", ToastLength.Short).Show ();
					} catch (Exception ex) {
						Console.WriteLine ("ex.Message");
						Console.WriteLine (ex.Message);
						Console.WriteLine ("ex.Message");
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

		private void InitMapFragment ()
		{
			mapFragment = (MapFragment)FragmentManager.FindFragmentById (Resource.Id.map);

			// mapFragment.View.Visibility = ViewStates.Gone;
			map = mapFragment.Map;

			if (mode == "info") {
				if (mapFragment != null && mapFragment.View != null) {
					mapFragment.View.Visibility = ViewStates.Gone;
				}
			}

			if (map != null) {
				map.MapType = GoogleMap.MapTypeTerrain;
				map.UiSettings.ZoomControlsEnabled = true;
				map.UiSettings.CompassEnabled = true;
				map.UiSettings.ScrollGesturesEnabled = true;

				GoogleMapOptions mapOptions = new GoogleMapOptions ()
					.InvokeCamera (CameraPosition.FromLatLngZoom (UPC, 16))
					.InvokeScrollGesturesEnabled (true)
					.InvokeMapType (GoogleMap.MapTypeNormal)
					.InvokeZoomControlsEnabled (true)
					.InvokeCompassEnabled (true);

				FragmentTransaction fragTx = FragmentManager.BeginTransaction ();
				mapFragment = MapFragment.NewInstance (mapOptions);
				fragTx.Add (Resource.Id.map, mapFragment, "map");
				fragTx.Commit ();

				/*CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
				builder.Target(UPC);
				builder.Zoom(16);
				CameraPosition cameraPosition = builder.Build();
				CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

				map.MoveCamera(cameraUpdate);*/

				MarkerOptions markerOptions = new MarkerOptions ();
				markerOptions.SetPosition (UPC);
				markerOptions.SetTitle ("CanchitApp");
				map.AddMarker (markerOptions);
			}
			/*_mapFragment = FragmentManager.FindFragmentByTag ("map") as MapFragment;
			if (_mapFragment == null) {
				GoogleMapOptions mapOptions = new GoogleMapOptions ()
					.InvokeCamera (CameraPosition.FromLatLngZoom (new LatLng (-12.103951800, -76.963278100), 16))
					.InvokeScrollGesturesEnabled (true)
					.InvokeMapType (GoogleMap.MapTypeNormal)
					.InvokeZoomControlsEnabled (true)
					.InvokeCompassEnabled (true);
				//	-12.103951800, -76.963278100

				FragmentTransaction fragTx = FragmentManager.BeginTransaction ();
				_mapFragment = MapFragment.NewInstance (mapOptions);
				fragTx.Add (Resource.Id.map, _mapFragment, "map");
				fragTx.Commit ();
			}*/
		}


		protected override void OnStart ()
		{
			base.OnStart ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			SetupMapIfNeeded ();
			locMgr = GetSystemService (Context.LocationService) as LocationManager;

			var locationCriteria = new Criteria ();
			locationCriteria.Accuracy = Accuracy.Coarse;
			locationCriteria.PowerRequirement = Power.Medium;
			string locationProvider = locMgr.GetBestProvider (locationCriteria, true);
			Console.WriteLine ("Starting location updates with " + locationProvider.ToString ());
			locMgr.RequestLocationUpdates (locationProvider, 2000, 1, this);
		}

		protected override void OnPause ()
		{
			base.OnPause ();

			// stop sending location updates when the application goes into the background
			// to learn about updating location in the background, refer to the Backgrounding guide
			// http://docs.xamarin.com/guides/cross-platform/application_fundamentals/backgrounding/


			// RemoveUpdates takes a pending intent - here, we pass the current Activity
			locMgr.RemoveUpdates (this);
		}

		protected override void OnStop ()
		{
			base.OnStop ();
		}

		public void OnLocationChanged (Android.Locations.Location location)
		{
			currentLocation = location;
			if (map != null) {
				Console.WriteLine ("Latitude: " + location.Latitude.ToString ());
				Console.WriteLine ("Longitude: " + location.Longitude.ToString ());
				Console.WriteLine ("Provider: " + location.Provider.ToString ());

				MarkerOptions markerOptions = new MarkerOptions ();
				markerOptions.SetPosition (new LatLng (location.Latitude, location.Longitude));
				markerOptions.SetTitle ("CanchitApp");
				map.AddMarker (markerOptions);

				GoogleMapOptions mapOptions = new GoogleMapOptions ()
				.InvokeCamera (CameraPosition.FromLatLngZoom (new LatLng (location.Latitude, location.Longitude), 16))
				.InvokeScrollGesturesEnabled (true)
				.InvokeMapType (GoogleMap.MapTypeNormal)
				.InvokeZoomControlsEnabled (true)
				.InvokeCompassEnabled (true);

				FragmentTransaction fragTx = FragmentManager.BeginTransaction ();
				mapFragment = MapFragment.NewInstance (mapOptions);
				fragTx.Add (Resource.Id.map, mapFragment, "map");
				fragTx.Commit ();
			}
		}

		public void OnProviderDisabled (string provider)
		{
			Console.WriteLine (provider + " disabled by user");

			if (map != null) {
				MarkerOptions markerOptions = new MarkerOptions ();
				markerOptions.SetPosition (UPC);
				markerOptions.SetTitle ("CanchitApp");
				map.AddMarker (markerOptions);
			}
		}

		public void OnProviderEnabled (string provider)
		{
			Console.WriteLine (provider + " enabled by user");
		}

		public void OnStatusChanged (string provider, Availability status, Bundle extras)
		{
			Console.WriteLine (provider + " availability has changed to " + status.ToString ());
		}

		private void SetupMapIfNeeded ()
		{
			if (mapFragment == null) {
				InitMapFragment ();
			}

			if (mode == "info") {
				if (mapFragment != null && mapFragment.View != null) {
					mapFragment.View.Visibility = ViewStates.Gone;
				}
			}

			if (map == null) {
				map = mapFragment.Map;
				if (map != null) {
					map = mapFragment.Map;

					if (map != null) {
						MarkerOptions markerOptions = new MarkerOptions ();
						markerOptions.SetPosition (UPC);
						markerOptions.SetTitle ("CheckinApp");
						map.AddMarker (markerOptions);
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

		async protected override void OnActivityResult (int requestCode, Result resultCode, Intent intent)
		{
			var self = this;
			var sharedPreferences = GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);

			if (requestCode == (int)RequestsConstants.AuthRequest) {
				if (resultCode == Result.Ok) {
					var authService = intent.GetStringExtra ("authService");

					if (authService == "Twitter") {
						var twitterClient = new CheckinShared.Twitter ("IO0mSObd1KnbSOkZXBvGchomD", "JiCrmSCOp0AR2m0zIjoY8Cq1STTbcjEPupMdpOkEihmHViQ5Lh");
						twitterClient.UserToken = sharedPreferences.GetString ("Twitter:token", "");
						twitterClient.UserSecret = sharedPreferences.GetString ("Twitter:secret", "");

						if (twitterClient.UserToken != "" && twitterClient.UserSecret != "") {
							try {
								string result = await twitterClient.UpdateStatus (new {
									status = "Estoy viendo " + movie.Title + " (" + "https://www.themoviedb.org/movie/" + movie.ApiId + ")"
								}) as string;

								Console.WriteLine ("result: " + result);

								Toast.MakeText (self, "Película compartida en Twitter", ToastLength.Short).Show ();
							} catch (Exception ex) {
								Console.WriteLine ("ex.Message");
								Console.WriteLine (ex.Message);
								Console.WriteLine ("ex.Message");
								Toast.MakeText (self, "Hubo un error al compartir la película: " + ex.Source, ToastLength.Long).Show ();
							}
						}
					} else if (authService == "Facebook") {
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

								Console.WriteLine ("result: " + result);

								Toast.MakeText (self, "Película compartida en Facebook", ToastLength.Short).Show ();
							} catch (Exception ex) {
								Console.WriteLine ("ex.StackTrace");
								Console.WriteLine (ex.StackTrace);
								Console.WriteLine ("ex.StackTrace");
								Toast.MakeText (self, "Hubo un error al compartir la película: " + ex.Source, ToastLength.Long).Show ();
							}
						}
					}
				}
			}
		}
	}
}

