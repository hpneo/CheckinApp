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

using CheckinShared.Models;
using CheckinShared.Services;

namespace CheckinAppAndroid
{
	[Activity (Label = "CheckinApp", Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]			
	public class MovieActivity : Activity
	{
		private CheckinShared.MovieDB movies;
		private CheckinShared.CheckinDB checkins;

		async protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			movies = new CheckinShared.MovieDB ();
			checkins = new CheckinShared.CheckinDB ();

			int movieId = this.Intent.GetIntExtra ("movieId", 0);
			string mode = this.Intent.GetStringExtra ("mode");
			var movie = movies.Get (movieId);

			TMDB api = new TMDB();
			if (movie.Overview == null) {
				JObject movieJSON = await api.Find(movie.ApiId) as JObject;
				movie.Overview = movieJSON ["overview"].ToString ();

				movies.Update (movie);
			}

			if (movie.Director == null) {
				JObject movieCreditsJSON = await api.GetCredits (movie.ApiId) as JObject;

				if (movieCreditsJSON ["crew"].Count() > 0) {
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

			buttonCheckin.Click += (object sender, EventArgs e) => {
				Checkin checkin = new Checkin();
				checkin.MovieId = movie.Id;
				checkin.CreatedAt = DateTime.UtcNow;

				checkins.Insert(checkin);

				Intent intent = new Intent();
				intent.PutExtra("checkinId", checkin.Id);
				intent.PutExtra("movieId", movie.Id);

				SetResult(Result.Ok, intent);
				Finish();
			};

			buttonShareFacebook.Click += async (object sender, EventArgs e) => {
				var sharedPreferences = GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);
				var facebookClient = new CheckinShared.Facebook ("1492339931014967", "7ae094df0f071a1972ed7c7354943f9a");

				facebookClient.UserToken = sharedPreferences.GetString("Facebook:token", "");
				string result = await facebookClient.PublishFeed (new {
					message = "Viendo " + movie.Title,
					link = "https://www.themoviedb.org/movie/" + movie.ApiId,
					picture = movie.PosterPath,
					name = movie.Title,
					caption = movie.Year,
					description = movie.Overview
				}) as string;

				if (result != null) {
					Toast.MakeText(Parent, "Película compartida", ToastLength.Short).Show();
				}
			};

			ActionBar.SetDisplayHomeAsUpEnabled (true);
			ActionBar.SetDisplayShowTitleEnabled (true);
			ActionBar.SetDisplayShowHomeEnabled (true);
			ActionBar.SetDisplayUseLogoEnabled (false);
			Window.SetTitle (movie.Title);
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == Android.Resource.Id.Home) {
				OnBackPressed ();
			}

			return base.OnOptionsItemSelected (item);
		}
	}
}

