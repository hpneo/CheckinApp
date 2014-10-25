
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
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
using CheckinShared.Models;

namespace CheckinAppAndroid
{
	[Activity (Label = "AddCatalogDialogFragment")]			
	public class AddCatalogDialogFragment : DialogFragment
	{
		private CheckinShared.CatalogDB catalogs;

		public override Dialog OnCreateDialog (Bundle bundle)
		{
			base.OnCreateDialog (bundle);

			catalogs = new CheckinShared.CatalogDB ();
			LayoutInflater inflater = Activity.LayoutInflater;
			View view = inflater.Inflate (Resource.Layout.AddCatalog, null);

			TextView textName = view.FindViewById<TextView> (Resource.Id.txtAgregarCatalogo);

			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.SetTitle ("Agregar Nuevo Catalogo");
			builder.SetView (view);
			builder.SetPositiveButton ("Salir", delegate (object sender, DialogClickEventArgs e) {
			});
			builder.SetNegativeButton ("Agregar", delegate (object sender, DialogClickEventArgs e) {
				Catalog catalog = new Catalog ();
				if (textName.Text.Length != 0)
				{
					catalog.Name = textName.Text;
					catalog.Quantity = 0;
					catalogs.Insert(catalog);

					( (ListCatalogActivity) Activity).ActualizarLista();
				}


			});


			return builder.Create ();
		}
	}
}

