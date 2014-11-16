using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Android.App;
using Android.Views;
using Android.Widget;

using System.IO;
using System.Net;
using System.Threading.Tasks;

using CheckinShared.Models;

namespace CheckinAppAndroid
{
	public class MovieCatalogAdapter : BaseAdapter
	{
		private CheckinShared.MovieDB movies;
		private ObservableCollection<MoviexCatalog> moviesList;
		private Activity activity;

		// private readonly object locker = new object();

		public MovieCatalogAdapter (Activity activity)
		{
			this.activity = activity;
			this.movies = new CheckinShared.MovieDB ();
			this.moviesList = new ObservableCollection<MoviexCatalog> ();
			this.moviesList.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e) {
				base.NotifyDataSetChanged ();
				// NotifyDataSetChanged();
			};
		}

		public override int Count {
			get { return moviesList.Count; }
		}

		public void Clear ()
		{
			moviesList.Clear ();
		}

		public void Add (MoviexCatalog movie)
		{
			moviesList.Add (movie);
		}

		public void AddAll (IList<MoviexCatalog> movies)
		{
			moviesList.Clear ();
			foreach (MoviexCatalog movie in movies) {
				moviesList.Add (movie);
			}
		}

		public void Insert (MoviexCatalog movie, int position)
		{
			moviesList.Insert (position, movie);
		}

		public void Remove (MoviexCatalog movie)
		{
			moviesList.Remove (movie);
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return new JavaObject<CheckinShared.Models.MoviexCatalog> (moviesList [position]);
		}

		public MoviexCatalog GetMovie (int position)
		{
			return (MoviexCatalog)moviesList [position];
		}

		public override long GetItemId (int position)
		{
			return (moviesList [position]).Id;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView;
			ViewHolder holder;

			if (view != null) {
				holder = view.Tag as ViewHolder;
			} else {
				holder = new ViewHolder ();
				view = activity.LayoutInflater.Inflate (Resource.Layout.ItemMovie, parent, false);
				holder.Title = view.FindViewById<TextView> (Resource.Id.textViewItemMovieTitle);
				holder.Image = view.FindViewById<ImageView> (Resource.Id.imageViewItemMoviePicture);
				view.Tag = holder;
			}

			MoviexCatalog moviexCatalog = ((MoviexCatalog)moviesList [position]);
			if (moviexCatalog.IdMovie != 0) {
				Movie movie = movies.Get (moviexCatalog.IdMovie);

				if (movie != null) {
					holder.Title.Text = movie.Title;

					if (movie.Year != null) {
						holder.Title.Text += " (" + movie.Year + ")";
					}

					Console.WriteLine ("PhotoPath: " + moviexCatalog.PhotoPath);

					if (moviexCatalog.Photo != null) {
						holder.Image.SetImageBitmap ((Android.Graphics.Bitmap)moviexCatalog.Photo);
					} else {
						if (moviexCatalog.PhotoPath.StartsWith ("/")) {
							moviexCatalog.Photo = BitmapHelpers.BitmapHelpers.LoadAndResizeBitmap (moviexCatalog.PhotoPath, 200, 200);
							holder.Image.SetImageBitmap ((Android.Graphics.Bitmap)moviexCatalog.Photo);
						} else {
							Koush.UrlImageViewHelper.SetUrlDrawable (holder.Image, moviexCatalog.PhotoPath);
							moviexCatalog.Photo = Koush.UrlImageViewHelper.GetCachedBitmap (moviexCatalog.PhotoPath);
						}
					}

					Console.WriteLine ("PhotoPath: " + moviexCatalog.Photo + "");
				}
			}

			return view;
		}

		private class ViewHolder : Java.Lang.Object
		{
			public TextView Title { get; set; }

			public ImageView Image { get; set; }
		}
	}
}

