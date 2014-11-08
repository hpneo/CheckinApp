
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

using CheckinShared.Models;
using Android.Views.Animations;

namespace CheckinAppAndroid
{
	[Activity (Label = "Catálogo", Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]			
	public class CatalogActivity : Activity
	{
		private CheckinShared.MovieDB movies;
		private CheckinShared.MoviexCatalogDB moviexcatalog;
		private Catalog catalog;
		private MoviesAdapter adapter;
		private ListView listViewMovies;

		public MoviesAdapter Adapter { get { return adapter; } }
		public ListView ListView { get { return listViewMovies; } }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			catalog = new Catalog ();
			catalog.Name = Intent.GetStringExtra ("Name");
			catalog.Id = Intent.GetIntExtra ("Id", -1);

			SetContentView (Resource.Layout.CheckinsFragment);

			listViewMovies = FindViewById<ListView> (Resource.Id.listViewMovies);
			adapter = new MoviesAdapter (this);
			listViewMovies.Adapter = adapter;

			listViewMovies.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Movie movie = adapter.GetMovie(e.Position);

				Intent intent = new Intent (this, typeof(MovieActivity));
				intent.PutExtra("movieId", movie.Id);
				intent.PutExtra("mode", "info");

				StartActivity(intent);
			};

			movies = new CheckinShared.MovieDB ();
			moviexcatalog = new CheckinShared.MoviexCatalogDB ();

			ActualizarLista ();

			ActionBar.SetDisplayHomeAsUpEnabled (true);
			ActionBar.SetDisplayShowTitleEnabled (true);
			ActionBar.SetDisplayShowHomeEnabled (true);
		}

		public override void OnAttachedToWindow() { 
			base.OnAttachedToWindow();
			Window.SetTitle (catalog.Name); 
		}
			
		public override bool OnCreateOptionsMenu(IMenu menu) {
			MenuInflater.Inflate (Resource.Menu.Main, menu);

			var addMenu = menu.Add (0, 1, 1, "Add");
			addMenu.SetIcon (Resource.Drawable.Add);
			addMenu.SetShowAsAction (ShowAsAction.IfRoom);

			var refreshMenu = menu.Add (0, 2, 2, "Refresh");
			refreshMenu.SetIcon (Resource.Drawable.Refresh);
			refreshMenu.SetShowAsAction (ShowAsAction.IfRoom);

			return base.OnCreateOptionsMenu (menu);
		}


		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == 1) {
				Intent intent = new Intent (this, typeof(AddMovieToCatalogActivity));
				intent.PutExtra ("Name", catalog.Name);
				intent.PutExtra ("Id", catalog.Id);
				StartActivityForResult (intent, 16);
			} else if (item.ItemId == 2) {
				Animation rotation = AnimationUtils.LoadAnimation (this, Resource.Animation.Rotate);

				rotation.RepeatCount = Animation.Infinite;

				ImageView imageView = (ImageView)LayoutInflater.Inflate (Resource.Layout.RefreshImageView, null);
				imageView.StartAnimation (rotation);

				item.SetActionView (imageView);

				ActualizarLista ();

				Handler handler = new Handler ();
				handler.PostDelayed (() => {
					imageView.ClearAnimation ();
					item.SetActionView (null);
				}, 1000);
			} else if (item.ItemId == 3) {
				Intent intent = new Intent (this, typeof(AuthActivity));
				StartActivityForResult (intent, 13);
			} else if (item.ItemId == Android.Resource.Id.Home) {
				OnBackPressed ();
			}
			return base.OnOptionsItemSelected (item);
		}

		public void ActualizarLista()
		{
			adapter.Clear ();
			foreach (MoviexCatalog moviexcatalogtemp in moviexcatalog.All()) {
				if(moviexcatalogtemp.IdCatalog == catalog.Id)
				{
					adapter.Add (movies.Get(moviexcatalogtemp.IdMovie));
				}
			}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			if (requestCode == 16) {
				if (resultCode == Result.Ok) {
					int movieId = intent.GetIntExtra ("movieId", 0);
					var movie = movies.Get (movieId);

					adapter.Add (movie);
					// adapter.NotifyDataSetChanged ();
				}
			}
		}
	}
}

