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
	[Activity (Label = "Agregar película", Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]		
	public class AddMovieToCatalogActivity : Activity
	{



		public override void OnAttachedToWindow() { 
			base.OnAttachedToWindow();
			this.Window.SetTitle ("Agregar película a " + Intent.GetStringExtra ("Name"));
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

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
				/*
				MoviexCatalog moviexcatalog = new MoviexCatalog();
				movies = new CheckinShared.MovieDB();
				moviexcatalogs = new CheckinShared.MoviexCatalogDB();
				catalogs = new CheckinShared.CatalogDB();

				// Intent intent = new Intent (this, typeof(MoviexCatalogActivity));



				Movie movie = adapter.GetMovie(e.Position);
				movie = movies.Insert(movie);
				moviexcatalog.IdMovie = movie.Id;
				int idCatalog = Intent.GetIntExtra("Id",-1);

				if(idCatalog != -1)
				{
					Catalog catalog = new Catalog();
					moviexcatalog.IdCatalog = idCatalog;
					moviexcatalogs.Insert(moviexcatalog);
					catalog = catalogs.Get(idCatalog);
					catalog.Quantity += 1;
					catalogs.Update(catalog);

					intent.PutExtra("movieId", movie.Id);

					SetResult(Result.Ok, intent);
					Finish();
				}
				else
				{
					SetResult(Result.Canceled, intent);
					Finish();
				}
				*/

				Intent intent = new Intent (this, typeof(AddMovieToCatalogDetailActivity));
				Movie movie = adapter.GetMovie(e.Position);
				intent.PutExtra ("MovieTitle",movie.Title);
				intent.PutExtra ("MovieDate",movie.Year);
				intent.PutExtra ("MovieDirector",movie.Director);
				intent.PutExtra ("MovieOverview",movie.Overview);
				intent.PutExtra ("MoviePosterPath",movie.PosterPath);

				intent.PutExtra ("Name",Intent.GetStringExtra ("Name"));
				intent.PutExtra ("Id",Intent.GetIntExtra("Id",-1));

				StartActivityForResult (intent, 30);
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

					if (movie.Overview == null) {
						JObject movieOverviewJSON = await api.Find (movie.ApiId) as JObject;
						movie.Overview = movieOverviewJSON ["overview"].ToString ();
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
					}

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

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == Android.Resource.Id.Home) {
				OnBackPressed ();
			}

			return base.OnOptionsItemSelected (item);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			if (requestCode == 30) {
				if (resultCode == Result.Ok) {
					int movieId = intent.GetIntExtra ("movieId", 0);
					SetResult(Result.Ok, intent);
					Finish();
				}
			}
		}
	}
}

