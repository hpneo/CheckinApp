using System;
using System.Collections;

using Android.App;
using Android.Views;
using Android.Widget;

using System.IO;
using System.Net;
using System.Threading.Tasks;

using CheckinShared.Models;

namespace CheckinApp
{
	public class MoviesAdapter : BaseAdapter
	{
		ArrayList moviesList;
		Activity activity;

		public MoviesAdapter (Activity activity, ArrayList moviesList)
		{
			this.activity = activity;
			this.moviesList = moviesList;
		}

		public override int Count {
			get { return moviesList.Count; }
		}

		public void Clear() {
			moviesList.Clear ();
		}

		public void Add(Movie movie) {
			moviesList.Add (movie);
		}

		public override Java.Lang.Object GetItem (int position) {
			return (Java.Lang.Object) moviesList [position];
		}

		public Movie GetMovie (int position) {
			return (Movie) moviesList [position];
		}

		public override long GetItemId (int position) {
			return ((Movie) moviesList [position]).Id;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView ?? activity.LayoutInflater.Inflate (Resource.Layout.ItemMovie, parent, false);

			var movieTitle = view.FindViewById<TextView> (Resource.Id.textViewItemMovieTitle);
			var movieImage = view.FindViewById<ImageView> (Resource.Id.imageViewItemMoviePicture);

			Movie movie = ((Movie)moviesList [position]);

			movieTitle.Text = movie.Title;

			if (movie.Poster != null) {
				movieImage.SetImageBitmap ((Android.Graphics.Bitmap) movie.Poster);
			} else {
				GetImageBitmapFromUrl (movie, movieImage);
			}

			return view;
		}

		private void GetImageBitmapFromUrl(Movie movie, ImageView imageView)
		{
			if (movie.Poster == null) {
				using (var webClient = new WebClient ()) {
					webClient.DownloadDataAsync (new Uri (movie.PosterPath));
					webClient.DownloadDataCompleted += delegate(object sender, DownloadDataCompletedEventArgs e) {
						var imageBytes = e.Result;

						if (imageBytes != null && imageBytes.Length > 0) {
							movie.Poster = Android.Graphics.BitmapFactory.DecodeByteArray (imageBytes, 0, imageBytes.Length);

							imageView.SetImageBitmap ((Android.Graphics.Bitmap) movie.Poster);
						}
					};
				}
			}
		}

	}
}

