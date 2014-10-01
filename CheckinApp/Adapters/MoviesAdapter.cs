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
	public class MoviesAdapter : BaseAdapter
	{
		private ObservableCollection<Movie> moviesList;
		private Activity activity;

		// private readonly object locker = new object();

		public MoviesAdapter (Activity activity) {
			this.activity = activity;
			this.moviesList = new ObservableCollection<Movie> ();
			this.moviesList.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e) {
				base.NotifyDataSetChanged();
				// NotifyDataSetChanged();
			};
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

		public void AddAll(IList<Movie> movies) {
			moviesList.Clear ();
			foreach (Movie movie in movies) {
				moviesList.Add (movie);
			}
		}

		public void Insert(Movie movie, int position) {
			moviesList.Insert (position, movie);
		}

		public void Remove(Movie movie) {
			moviesList.Remove (movie);
		}

		public override Java.Lang.Object GetItem (int position) {
			return new JavaObject<CheckinShared.Models.Movie> (moviesList [position]);
		}

		public Movie GetMovie (int position) {
			return (Movie) moviesList [position];
		}

		public override long GetItemId (int position) {
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

			Movie movie = ((Movie)moviesList [position]);

			if (movie != null) {
				holder.Title.Text = movie.Title;

				if (movie.Year != null) {
					holder.Title.Text += " (" + movie.Year + ")";
				}

				if (movie.Poster != null) {
					holder.Image.SetImageBitmap ((Android.Graphics.Bitmap)movie.Poster);
				} else {
					Koush.UrlImageViewHelper.SetUrlDrawable (holder.Image, movie.PosterPath);
					movie.Poster = Koush.UrlImageViewHelper.GetCachedBitmap (movie.PosterPath);
				}
			}

			return view;
		}

		private class ViewHolder : Java.Lang.Object {
			public TextView Title { get; set; }
			public ImageView Image { get; set; }
		}
	}
}

