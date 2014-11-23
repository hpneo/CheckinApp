using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using CheckinShared.Models;	

namespace CheckinAppAndroid
{
	public class CheckinsFragment : Android.Support.V4.App.Fragment
	{
		private CheckinShared.MovieDB movies;
		private CheckinShared.CheckinDB checkins;
		private MoviesAdapter adapter;
		private ListView listViewMovies;

		public CheckinsFragment ()
		{
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View rootView = inflater.Inflate(Resource.Layout.CheckinsFragment, container, false);

			listViewMovies = rootView.FindViewById<ListView> (Resource.Id.listViewMovies);

			adapter = new MoviesAdapter (Activity);
			listViewMovies.Adapter = adapter;

			listViewMovies.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
				Movie movie = adapter.GetMovie(e.Position);

				Intent intent = new Intent (Activity, typeof(MovieActivity));
				intent.PutExtra("movieId", movie.Id);
				intent.PutExtra("mode", "info");

				StartActivity(intent);
			};

			listViewMovies.ItemLongClick += delegate(object sender, AdapterView.ItemLongClickEventArgs e) {
				DeleteMovieDialogFragment dialog = new DeleteMovieDialogFragment();
				dialog.Movie = adapter.GetMovie(e.Position);

				dialog.Show(FragmentManager, "DeleteMovieDialogFragment");
			};

			movies = new CheckinShared.MovieDB ();
			checkins = new CheckinShared.CheckinDB ();

			// Toast.MakeText(Activity, movies.Count() + " películas en tu colección", ToastLength.Long).Show();

			RefreshList ();

			return rootView;
		}

		public void RefreshList()
		{
			adapter.Clear ();

			Toast.MakeText(Activity, checkins.Count() + " películas en tu colección", ToastLength.Long).Show();

			foreach (Checkin checkin in checkins.All()) {
				if (checkin.Movie != null) {
					adapter.Add (checkin.Movie);
				}
			}
		}

		public void AddMovie(int movieId) {
			var movie = movies.Get (movieId);

			adapter.Add (movie);
			Console.WriteLine ("Movies count in Main: " + movies.All ().Count ());
		}

		public void RemoveMovie(Movie movie) {
			adapter.Remove (movie);
		}

		protected void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			Console.WriteLine ("resultCode: " + (int) resultCode);
			Console.WriteLine ("Result.Ok: " + (int) Result.Ok);
			Console.WriteLine("Movies count in Main: " + movies.All().Count());
			if ((int) resultCode == (int) Result.Ok) {
				int movieId = intent.GetIntExtra ("movieId", 0);
				var movie = movies.Get (movieId);

				adapter.Add (movie);
				Console.WriteLine("Movies count in Main: " + movies.All().Count());
			}
		}
	}
}

