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
		private CheckinShared.MoviexCatalogDB moviexcatalogs;
		private CheckinShared.MovieDB movies;
		private CheckinShared.CatalogDB catalogs;


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

			ListView listViewMovies = FindViewById<ListView> (Resource.Id.listView2);

			MoviesAdapter adapter = new MoviesAdapter (this);

			listViewMovies.Adapter = adapter;
			listViewMovies.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
				MoviexCatalog moviexcatalog = new MoviexCatalog();

				movies = new CheckinShared.MovieDB();
				moviexcatalogs = new CheckinShared.MoviexCatalogDB();
				catalogs = new CheckinShared.CatalogDB();

				// Intent intent = new Intent (this, typeof(MoviexCatalogActivity));

				Intent intent = new Intent();

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

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == Android.Resource.Id.Home) {
				OnBackPressed ();
			}

			return base.OnOptionsItemSelected (item);
		}
	}
}

