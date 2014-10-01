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
		private MoviesAdapter adapter;
		private ListView listViewMovies;

		public MoviesAdapter Adapter { get { return adapter; } }
		public ListView ListView { get { return listViewMovies; } }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			listViewMovies = FindViewById<ListView> (Resource.Id.listViewMovies);
			adapter = new MoviesAdapter (this);

			listViewMovies.Adapter = adapter;

			//var context = this;

			listViewMovies.ItemLongClick += delegate(object sender, AdapterView.ItemLongClickEventArgs e) {
				DeleteMovieDialogFragment dialog = new DeleteMovieDialogFragment();
				dialog.Movie = adapter.GetMovie(e.Position);
				dialog.Show(FragmentManager, "DeleteMovieDialogFragment");
//				AlertDialog.Builder builder = new AlertDialog.Builder(this);
//
//				AlertDialog alertDialog = builder.Create();
//				alertDialog.SetView(new TextView(context));
//
//				alertDialog.Show();
			};

			movies = new CheckinShared.MovieDB ();
			Toast.MakeText(this, movies.Count() + " películas en tu colección", ToastLength.Long).Show();

			foreach (Movie movie in movies.All()) {
				adapter.Add (movie);
			}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			if (resultCode == Result.Ok) {
				int movieId = intent.GetIntExtra ("movieId", 0);
				var movie = movies.Get (movieId);

				adapter.Add (movie);
				// adapter.NotifyDataSetChanged ();
			}
		}

		public override bool OnCreateOptionsMenu(IMenu menu) {
			MenuInflater.Inflate (Resource.Menu.Main, menu);

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

			var facebookMenu = menu.Add (1, 3, 3, "Authorize Facebook");
			facebookMenu.SetIcon (Resource.Drawable.Facebook);
			facebookMenu.SetShowAsAction (ShowAsAction.IfRoom);

			var twitterMenu = menu.Add (1, 4, 4, "Authorize Twitter");
			twitterMenu.SetIcon (Resource.Drawable.Twitter);
			twitterMenu.SetShowAsAction (ShowAsAction.IfRoom);

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == 1) {
				Intent intent = new Intent (this, typeof(AddMovie));
				StartActivityForResult (intent, 1);
			} else if (item.ItemId == 2) {
				Animation rotation = AnimationUtils.LoadAnimation (this, Resource.Animation.Rotate);
				adapter.Clear ();

				rotation.RepeatCount = Animation.Infinite;

				ImageView imageView = (ImageView)LayoutInflater.Inflate (Resource.Layout.RefreshImageView, null);
				imageView.StartAnimation (rotation);

				item.SetActionView (imageView);

				foreach (Movie movie in movies.All()) {
					adapter.Add (movie);
				}

				Handler handler = new Handler ();
				handler.PostDelayed (() => {
					imageView.ClearAnimation ();
					item.SetActionView (null);
				}, 1000);
			} else if (item.ItemId == 3) {
				Intent intent = new Intent (this, typeof(AuthActivity));
				StartActivityForResult (intent, 1);
				// AuthFragment dialog = new AuthFragment();
				// dialog.Show(FragmentManager, "AuthFragment");
			}
			return base.OnOptionsItemSelected (item);
		}
	}
}


