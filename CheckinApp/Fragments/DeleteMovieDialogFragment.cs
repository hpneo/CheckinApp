using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace CheckinAppAndroid
{
	public class DeleteMovieDialogFragment : DialogFragment
	{
		public CheckinShared.Models.Movie Movie { get; set; }

		public override Dialog OnCreateDialog(Bundle savedInstanceState) {
			base.OnCreateDialog (savedInstanceState);

			LayoutInflater inflater = Activity.LayoutInflater;

			View view = inflater.Inflate (Resource.Layout.DeleteMovie, null);

			ImageView imageViewDeleteMoviePicture = view.FindViewById<ImageView> (Resource.Id.imageViewDeleteMoviePicture);
			TextView textViewDeleteMovieTitle = view.FindViewById<TextView> (Resource.Id.textViewDeleteMovieTitle);

			if (Movie != null) {
				if (Movie.Poster != null) {
					imageViewDeleteMoviePicture.SetImageBitmap ((Android.Graphics.Bitmap) Movie.Poster);
				}

				textViewDeleteMovieTitle.Text = Movie.Title;

				if (Movie.Year != null) {
					textViewDeleteMovieTitle.Text += " (" + Movie.Year + ")";
				}
			}

			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.SetTitle ("¿Desea eliminar esta película?");
			builder.SetView (view);
			builder.SetPositiveButton ("Sí", delegate (object sender, DialogClickEventArgs e) {
				Console.WriteLine ("Sí");
				CheckinShared.MovieDB movies = new CheckinShared.MovieDB ();

				if (movies.Delete(Movie)) {
					((MainActivity) Activity).Adapter.Remove(Movie);
					((MainActivity) Activity).Adapter.NotifyDataSetChanged();

					Toast.MakeText(Activity, "Película eliminada", ToastLength.Long).Show();
				}
			});
			builder.SetNegativeButton ("No", delegate (object sender, DialogClickEventArgs e) {
				Console.WriteLine ("No");
			});

			return builder.Create ();
		}
	}
}

