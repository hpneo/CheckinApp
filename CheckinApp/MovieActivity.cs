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

				movie.Director = movieCreditsJSON ["crew"] [0] ["name"].ToString ();
				movie.Cast = movieCreditsJSON ["cast"] [0] ["name"].ToString () + "\n" +
					movieCreditsJSON ["cast"] [1] ["name"].ToString () + "\n" +
					movieCreditsJSON ["cast"] [2] ["name"].ToString () + "\n" +
					movieCreditsJSON ["cast"] [3] ["name"].ToString () + "\n" +
					movieCreditsJSON ["cast"] [4] ["name"].ToString () + "\n" +
					movieCreditsJSON ["cast"] [5] ["name"].ToString ();

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

			if (mode == "info") {
				buttonCheckin.Visibility = ViewStates.Gone;
			}

			textViewMovieTitle.Text = movie.Title;
			textViewMovieYear.Text = movie.Year;
			textViewMovieDescription.Text = movie.Overview;
			textViewMovieDirector.Text = movie.Director;
			textViewMovieCast.Text = movie.Cast;

			if (movie.Poster != null) {
				imageViewMoviePoster.SetImageBitmap ((Android.Graphics.Bitmap)movie.Poster);
			} else {
				Koush.UrlImageViewHelper.SetUrlDrawable (imageViewMoviePoster, movie.PosterPath);
				movie.Poster = Koush.UrlImageViewHelper.GetCachedBitmap (movie.PosterPath);
			}

			buttonCheckin.Click += (object sender, EventArgs e) => {
				Checkin checkin = new Checkin();
				checkin.MovieId = movie.Id;

				checkins.Insert(checkin);

				Intent intent = new Intent();
				intent.PutExtra("checkinId", checkin.Id);
				intent.PutExtra("movieId", movie.Id);

				SetResult(Result.Ok, intent);
				Finish();
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

