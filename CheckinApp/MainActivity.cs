	using System;
using System.IO;
using System.Collections;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;

using Android.Webkit;

using SQLite;

using CheckinShared.Models;

namespace CheckinAppAndroid
{
	[Activity (Label = "CheckinApp", MainLauncher = true, Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]
	public class MainActivity : Activity
	{
		private CheckinShared.MovieDB movies;
		private CheckinShared.CheckinDB checkins;
		private MoviesAdapter adapter;
		private ListView listViewMovies;

		public MoviesAdapter Adapter { get { return adapter; } }
		public ListView ListView { get { return listViewMovies; } }

		int category;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			listViewMovies = FindViewById<ListView> (Resource.Id.listViewMovies);
			adapter = new MoviesAdapter (this);
			listViewMovies.Adapter = adapter;
			//var context = this;

			listViewMovies.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Movie movie = adapter.GetMovie(e.Position);

				Intent intent = new Intent (this, typeof(MovieActivity));
				intent.PutExtra("movieId", movie.Id);
				intent.PutExtra("mode", "info");

				StartActivity(intent);
			};

			listViewMovies.ItemLongClick += delegate(object sender, AdapterView.ItemLongClickEventArgs e) {
				DeleteMovieDialogFragment dialog = new DeleteMovieDialogFragment();
				dialog.Movie = adapter.GetMovie(e.Position);
				dialog.Show(FragmentManager, "DeleteMovieDialogFragment");
//				AlertDialog.Builder builder = new AlertDialog.Builder(this);//
//				AlertDialog alertDialog = builder.Create();
//				alertDialog.SetView(new TextView(context));//
//				alertDialog.Show();
			};

			movies = new CheckinShared.MovieDB ();
			checkins = new CheckinShared.CheckinDB ();
			Toast.MakeText(this, movies.Count() + " películas en tu colección", ToastLength.Long).Show();

			foreach (Checkin checkin in checkins.All()) {
				if (checkin.Movie != null) {
					adapter.Add (checkin.Movie);
				}
			}

			/*foreach (Movie movie in movies.All()) {
				adapter.Add (movie);
			}*/

			Console.WriteLine("Movies count in Main: " + movies.All().Count());

			if (bundle != null) {
				category = bundle.GetInt ("Películas");
				ActionBar.SelectTab (ActionBar.GetTabAt (category));
			}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			if (requestCode == 11) {
				if (resultCode == Result.Ok) {
					int movieId = intent.GetIntExtra ("movieId", 0);
					var movie = movies.Get (movieId);

					adapter.Add (movie);
					Console.WriteLine("Movies count in Main: " + movies.All().Count());
					// adapter.NotifyDataSetChanged ();
				}
			}

			if (requestCode == 13) {
				// var token = intent.GetStringExtra ("token");
			}

			if (requestCode == 14) {

				ActionBar.SelectTab (ActionBar.GetTabAt(category));
			}
		}
			

		public override bool OnCreateOptionsMenu(IMenu menu) {
			MenuInflater.Inflate (Resource.Menu.Main, menu);
			ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			// "Creación de Tabs"

			ActionBar.Tab tab = ActionBar.NewTab();
			tab.SetText("Peliculas");
			tab.TabSelected += (sender, args) => {
			};
			ActionBar.AddTab(tab);

			tab = ActionBar.NewTab();
			tab.SetText("Catálogo");
			tab.TabSelected += (sender, args) => {
				Intent intent = new Intent (this, typeof(ListCatalogActivity));
				StartActivityForResult (intent, 14);
			};
			ActionBar.AddTab(tab);


			// Finaliza "Creación de Tabs"

			/*var searchMenu = menu.Add ("Search");
			searchMenu.SetVisible (true);
			searchMenu.SetShowAsAction (ShowAsAction.IfRoom);
			searchMenu.SetActionView(new SearchView(this));*/

			var addMenu = menu.Add (0, 1, 1, "Add");
			addMenu.SetIcon (Resource.Drawable.Add);
			addMenu.SetShowAsAction (ShowAsAction.IfRoom);

			var refreshMenu = menu.Add (0, 2, 2, "Refresh");
			refreshMenu.SetIcon (Resource.Drawable.Refresh);
			refreshMenu.SetShowAsAction (ShowAsAction.IfRoom);

			/*var facebookMenu = menu.Add (1, 3, 3, "Authorize Facebook");
			facebookMenu.SetIcon (Resource.Drawable.Facebook);
			facebookMenu.SetShowAsAction (ShowAsAction.IfRoom);

			var twitterMenu = menu.Add (1, 4, 4, "Authorize Twitter");
			twitterMenu.SetIcon (Resource.Drawable.Twitter);
			twitterMenu.SetShowAsAction (ShowAsAction.IfRoom);*/

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == 1) {
				Intent intent = new Intent (this, typeof(AddMovieActivity));
				StartActivityForResult (intent, 11);
			} else if (item.ItemId == 2) {
				Animation rotation = AnimationUtils.LoadAnimation (this, Resource.Animation.Rotate);
				adapter.Clear ();

				rotation.RepeatCount = Animation.Infinite;

				ImageView imageView = (ImageView)LayoutInflater.Inflate (Resource.Layout.RefreshImageView, null);
				imageView.StartAnimation (rotation);

				item.SetActionView (imageView);

				foreach (Checkin checkin in checkins.All()) {
					if (checkin.Movie != null) {
						adapter.Add (checkin.Movie);
					}
				}

				Console.WriteLine("Movies count in Main: " + movies.All().Count());

				Handler handler = new Handler ();
				handler.PostDelayed (() => {
					imageView.ClearAnimation ();
					item.SetActionView (null);
				}, 1000);
			} else if (item.ItemId == 3) {
				Intent intent = new Intent (this, typeof(AuthActivity));
				StartActivityForResult (intent, 13);
			}
			return base.OnOptionsItemSelected (item);
		}
	}
}


