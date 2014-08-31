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

namespace CheckinApp
{
	[Activity (Label = "Add Movie", Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]			
	public class AddMovie : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.AddMovie);

			string dbPath = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "checkindb.db3");
			var db = new SQLiteConnection (dbPath);

			this.SetProgressBarIndeterminateVisibility (false);
			this.SetProgressBarVisibility (false);

			ProgressBar progressbar1 = FindViewById<ProgressBar> (Resource.Id.progressBar1);
			progressbar1.Visibility = ViewStates.Gone;

			SearchView searchView1 = FindViewById<SearchView> (Resource.Id.searchView1);

			ListView listView2 = FindViewById<ListView> (Resource.Id.listView2);

			MoviesAdapter adapter = new MoviesAdapter (this, new ArrayList ());

			listView2.Adapter = adapter;
			listView2.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
				Movie movie = adapter.GetMovie(e.Position);
				db.Insert(movie);

				Intent intent = new Intent();
				intent.PutExtra("movieId", movie.Id);

				SetResult(Result.Ok, intent);
				Finish();
			};

			searchView1.QueryTextSubmit += async delegate(object sender, SearchView.QueryTextSubmitEventArgs e) {
				Console.WriteLine(searchView1.Query);
				progressbar1.Visibility = ViewStates.Visible;

				Task<JObject> resultsTask = searchMovies(searchView1.Query);

				JObject results = await resultsTask;

				JArray movies = (JArray)results["results"];

				Console.WriteLine(movies.Count + " movies count");
				adapter.Clear();

				foreach (var movieJSON in movies) {
					Movie movie = new Movie();
					movie.Title = movieJSON["title"].ToString();
					movie.PosterPath = "http://image.tmdb.org/t/p/w154" + movieJSON["poster_path"].ToString();

					adapter.Add(movie);
				}

				progressbar1.Visibility = ViewStates.Gone;
			};

			searchView1.Close += delegate(object sender, SearchView.CloseEventArgs e) {
				progressbar1.Visibility = ViewStates.Gone;
			};
		}

		public async Task<JObject> searchMovies(string query) {
			var httpClient = new WebClient ();
			Task<string> contentTask = httpClient.DownloadStringTaskAsync (new Uri ("https://api.themoviedb.org/3/search/movie?api_key=fdf3c94669f3cc0906fddc99e5cd8208&query=" + query));

			string contentJSON = await contentTask;
			var content = JObject.Parse (contentJSON);

			return content;
		}
	}
}

