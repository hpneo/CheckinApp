using System;
using System.IO;
using System.Collections;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using SQLite;

namespace CheckinApp
{
	[Activity (Label = "CheckinApp", MainLauncher = true, Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]
	public class MainActivity : Activity
	{
		private SQLiteConnection db;
		private MoviesAdapter adapter;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			string dbPath = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "checkindb.db3");
			db = new SQLiteConnection (dbPath);
			db.CreateTable<Movie> ();

			ListView listView1 = FindViewById<ListView> (Resource.Id.listView1);
			adapter = new MoviesAdapter (this, new ArrayList ());

			listView1.Adapter = adapter;

			var table = db.Table<Movie> ();

			foreach (Movie movie in table) {
				adapter.Add (movie);
			}

			Toast.MakeText(this, db.Table<Movie>().Count() + " movies in db", ToastLength.Long).Show();

			/*this.ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			var tabMovies = this.ActionBar.NewTab ().SetText ("Movies");
			var tabSeries = this.ActionBar.NewTab ().SetText ("Series");

			tabMovies.TabSelected += delegate (object sender, ActionBar.TabEventArgs e) {
				//
			};

			tabSeries.TabSelected += delegate (object sender, ActionBar.TabEventArgs e) {
				//
			};

			this.ActionBar.AddTab (tabMovies);
			this.ActionBar.AddTab (tabSeries);*/
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			if (resultCode == Result.Ok) {
				int movieId = intent.GetIntExtra ("movieId", 0);
				var movie = db.Get<Movie> (movieId);

				adapter.Add (movie);
				adapter.NotifyDataSetChanged ();
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

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == 1) {
				Intent intent = new Intent (this, typeof(AddMovie));
				StartActivityForResult (intent, 1);
			}
			return base.OnOptionsItemSelected (item);
		}
	}
}


