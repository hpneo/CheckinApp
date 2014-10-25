using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;

using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using CheckinShared.Models;
using CheckinShared.Services;

namespace CheckinAppAndroid
{
	[Activity (Label = "Add Movie", Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]			
	public class AddMovieActivity : Activity
	{
		private CheckinShared.MovieDB movies;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			movies = new CheckinShared.MovieDB ();

			SetContentView (Resource.Layout.AddMovie);

			this.SetProgressBarIndeterminateVisibility (false);
			this.SetProgressBarVisibility (false);

			ProgressBar progressbarSearch = FindViewById<ProgressBar> (Resource.Id.progressBar1);
			progressbarSearch.Visibility = ViewStates.Gone;

			SearchView searchViewMovie = FindViewById<SearchView> (Resource.Id.searchView1);

			ListView listView2 = FindViewById<ListView> (Resource.Id.listView2);

			MoviesAdapter adapter = new MoviesAdapter (this);

			listView2.Adapter = adapter;
			listView2.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
				Movie movie = adapter.GetMovie(e.Position);

				int count = movies.All().Where(m => m.ApiId.Equals(movie.ApiId)).Count();

				if (count == 0) {
					movies.Insert(movie);
				}
				else {
					movie = movies.All().Where(m => m.ApiId.Equals(movie.ApiId)).First();
				}

				Console.WriteLine("Movie count: " + count);

				Intent intent = new Intent (this, typeof(MovieActivity));
				intent.PutExtra("movieId", movie.Id);
				StartActivityForResult (intent, 101);
				/*Movie movie = adapter.GetMovie(e.Position);
				movies.Insert(movie);

				Intent intent = new Intent();
				intent.PutExtra("movieId", movie.Id);

				SetResult(Result.Ok, intent);
				Finish();*/
			};

			searchViewMovie.QueryTextSubmit += async delegate(object sender, SearchView.QueryTextSubmitEventArgs e) {
				Console.WriteLine(searchViewMovie.Query);
				progressbarSearch.Visibility = ViewStates.Visible;

				TMDB api = new TMDB();
				Task<object> resultsTask = api.SearchMovies(searchViewMovie.Query);

				JObject results = await resultsTask as JObject;

				JArray moviesArray = (JArray)results["results"];

				Console.WriteLine(moviesArray.Count + " movies count");
				adapter.Clear();

				foreach (var movieJSON in moviesArray) {
					Movie movie = new Movie();
					movie.Title = movieJSON["title"].ToString();
					movie.Year = movieJSON["release_date"].ToString().Split(new char[]{ '-' })[0];
					movie.PosterPath = "http://image.tmdb.org/t/p/w154" + movieJSON["poster_path"].ToString();
					movie.ApiId = movieJSON["id"].ToString();

					adapter.Add(movie);
				}

				progressbarSearch.Visibility = ViewStates.Gone;
			};

			searchViewMovie.Close += delegate(object sender, SearchView.CloseEventArgs e) {
				progressbarSearch.Visibility = ViewStates.Gone;
			};

			ActionBar.SetDisplayHomeAsUpEnabled (true);
			ActionBar.SetDisplayShowTitleEnabled (true);
			ActionBar.SetDisplayShowHomeEnabled (true);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			if (requestCode == 101) {
				if (resultCode == Result.Ok) {
					SetResult(Result.Ok, intent);
					Finish();
				}
			}
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == Android.Resource.Id.Home) {
				OnBackPressed ();
			}

			return base.OnOptionsItemSelected (item);
		}
	}
}

