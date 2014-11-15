
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using CheckinShared;
using CheckinShared.Services;
using Android.Provider;
using BitmapHelpers;
using Java.IO;
using CheckinShared.Models;

using Uri = Android.Net.Uri;
using Environment = Android.OS.Environment;
using Android.Content.PM;

namespace CheckinAppAndroid
{
	[Activity (Label = "AddMovieToCatalogDetailActivity", Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]			
	public class AddMovieToCatalogDetailActivity : Activity
	{
		public static class Camera{
			public static File _file;
			public static File _dir;     
			public static Bitmap bitmap;
		}

		private CheckinShared.MoviexCatalogDB moviexcatalogs;
		private CheckinShared.MovieDB movies;
		private CheckinShared.CatalogDB catalogs;

		private CheckinShared.Models.Movie movie;
		private int idCatalog;

		ImageView imgFoto;

		public override void OnAttachedToWindow() { 
			base.OnAttachedToWindow();
			this.Window.SetTitle ("Agregar Película a Catálogo");
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			catalogs = new CheckinShared.CatalogDB ();

			SetContentView (Resource.Layout.AddMovieToCatalog);
			idCatalog = Intent.GetIntExtra ("Id", -1);

			movie = new CheckinShared.Models.Movie ();

			movie.Title = Intent.GetStringExtra ("MovieTitle");
			movie.Year = Intent.GetStringExtra ("MovieDate");
			movie.Director = Intent.GetStringExtra ("MovieDirector");
			movie.Overview = Intent.GetStringExtra ("MovieOverview");
			movie.PosterPath = Intent.GetStringExtra ("MoviePosterPath");

			TextView txtTitulo = FindViewById<TextView> (Resource.Id.txtTituloAgregarPelicula);
			txtTitulo.Text = "Agregar '" + Intent.GetStringExtra ("MovieTitle") + "' a '" + Intent.GetStringExtra ("Name")+"'";

			EditText textNombre = FindViewById<EditText> (Resource.Id.txtNombrePelicula);
			textNombre.Text += movie.Title;

			EditText textFecha = FindViewById<EditText> (Resource.Id.txtAñoEstrenoPelicula);
			if (movie.Year != null) {
				textFecha.Text +=  movie.Year;
			}

			EditText textDirector = FindViewById<EditText> (Resource.Id.txtDirectorPelicula);
			textDirector.Text += movie.Director;

			EditText textDescripcion = FindViewById<EditText> (Resource.Id.txtDescripcion);
			textDescripcion.Text += movie.Overview;

			imgFoto = FindViewById<ImageView> (Resource.Id.imgFoto);
			if (movie.PosterPath != null) {
				Koush.UrlImageViewHelper.SetUrlDrawable (imgFoto, movie.PosterPath);
			}

			Button btnGuardar = FindViewById<Button> (Resource.Id.btnGuardarPelicula);
			Button btnCancelar = FindViewById<Button> (Resource.Id.btnCancelarPelicula);

			btnGuardar.Click += (object sender, EventArgs e) => {
				MoviexCatalog moviexcatalog = new MoviexCatalog();
				movies = new CheckinShared.MovieDB();
				moviexcatalogs = new CheckinShared.MoviexCatalogDB();
				catalogs = new CheckinShared.CatalogDB();

				Intent intent = new Intent();

				movie = movies.Insert(movie);
				moviexcatalog.IdMovie = movie.Id;

				if(idCatalog != -1)
				{
					Catalog catalog = new Catalog();
					moviexcatalog.IdCatalog = idCatalog;
					moviexcatalogs.Insert(moviexcatalog);
					catalog = catalogs.Get(idCatalog);
					catalog.Quantity += 1;
					catalogs.Update(catalog);

					intent.PutExtra("movieId", movie.Id);
				}
				SetResult(Result.Ok, intent);
				Finish();
			};

			btnCancelar.Click += (object sender, EventArgs e) => {

				Intent intent = new Intent();

				SetResult(Result.Canceled, intent);
				Finish();
			};


			imgFoto.Click += (object sender, EventArgs e) => {

				if (IsThereAnAppToTakePictures())
				{
					CreateDirectoryForPictures();

					TakeAPicture(sender,e);
				}

			};

			ActionBar.SetDisplayHomeAsUpEnabled (true);
			ActionBar.SetDisplayShowTitleEnabled (true);
			ActionBar.SetDisplayShowHomeEnabled (true);
				

		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile(Camera._file);
			mediaScanIntent.SetData(contentUri);
			SendBroadcast(mediaScanIntent);

			int height = imgFoto.Height;
			int width = imgFoto.Width ;
			Camera.bitmap = Camera._file.Path.LoadAndResizeBitmap (width, height);

			imgFoto.SetImageBitmap (Camera.bitmap);

		}

		private void CreateDirectoryForPictures()
		{
			Camera._dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "CheckinApp");
			if (!Camera._dir.Exists())
			{
				Camera._dir.Mkdirs();
			}
		}

		private bool IsThereAnAppToTakePictures()
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		private void TakeAPicture(object sender, EventArgs eventArgs)
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);

			Camera._file = new File(Camera._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));

			intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(Camera._file));

			StartActivityForResult(intent, 0);
		}

		public override bool OnCreateOptionsMenu(IMenu menu) {
			MenuInflater.Inflate (Resource.Menu.Main, menu);

			var addMenu = menu.Add (0, 1, 1, "Add");
			addMenu.SetIcon (Resource.Drawable.Add);
			addMenu.SetShowAsAction (ShowAsAction.IfRoom);

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == 1) {
				MoviexCatalog moviexcatalog = new MoviexCatalog ();
				movies = new CheckinShared.MovieDB ();
				moviexcatalogs = new CheckinShared.MoviexCatalogDB ();
				catalogs = new CheckinShared.CatalogDB ();

				Intent intent = new Intent ();
				movie.Poster = Camera.bitmap;
				movie = movies.Insert (movie);
				moviexcatalog.IdMovie = movie.Id;

				if (idCatalog != -1) {
					Catalog catalog = new Catalog ();
					moviexcatalog.IdCatalog = idCatalog;
					moviexcatalogs.Insert (moviexcatalog);
					catalog = catalogs.Get (idCatalog);
					catalog.Quantity += 1;
					catalogs.Update (catalog);

					intent.PutExtra ("movieId", movie.Id);
				}
				SetResult (Result.Ok, intent);
				Finish ();
			}
			return base.OnOptionsItemSelected (item);
		}

	}
}

