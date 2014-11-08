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

using CheckinShared;
using CheckinShared.Models;

namespace CheckinAppAndroid
{
	[Activity (Label = "DeleteCatalogDialogFragment")]			
	public class DeleteCatalogDialogFragment : Android.Support.V4.App.DialogFragment
	{
		public CheckinShared.Models.Catalog Catalog { get; set; }

		public override Dialog OnCreateDialog (Bundle bundle)
		{
			base.OnCreateDialog (bundle);

			LayoutInflater inflater = Activity.LayoutInflater;
			View view = inflater.Inflate (Resource.Layout.DeleteCatalog, null);

			TextView textViewDeleteCatalogName = view.FindViewById<TextView> (Resource.Id.textViewDeleteCatalogName);
			TextView textViewDeleteCatalogQuantity = view.FindViewById<TextView> (Resource.Id.textViewDeleteCatalogQuantity);

			if (Catalog != null) {
				textViewDeleteCatalogName.Text = "Nombre: " + Catalog.Name;
				textViewDeleteCatalogQuantity.Text = "Cantidad de Peliculas: " + Catalog.Quantity.ToString();
			}

			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.SetTitle ("¿Desea eliminar este catálogo?");
			builder.SetView (view);
			builder.SetPositiveButton ("Sí", delegate (object sender, DialogClickEventArgs e) {
				CheckinShared.CatalogDB catalogs = new CheckinShared.CatalogDB ();
				int IdCatalog = Catalog.Id;
				if (catalogs.Delete(Catalog)) {
					CheckinShared.MoviexCatalogDB moviexcatalogs = new MoviexCatalogDB();

					foreach(MoviexCatalog moviexcatalog in moviexcatalogs.All())
					{
						if(moviexcatalog.IdCatalog == IdCatalog)
						{
							moviexcatalogs.Delete(moviexcatalog);
						}
					}

					((CatalogsFragment) ((MainActivity) Activity).CurrentFragment()).Adapter.Remove(Catalog);

					Toast.MakeText(Activity, "Catálogo eliminado", ToastLength.Long).Show();
				}
			});
			builder.SetNegativeButton ("No", delegate (object sender, DialogClickEventArgs e) {
			});

			return builder.Create ();
		}
	}
}

