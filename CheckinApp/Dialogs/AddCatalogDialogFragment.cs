﻿
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
	public class AddCatalogDialogFragment : Android.Support.V4.App.DialogFragment
	{
		private CheckinShared.CatalogDB catalogs;

		public override Dialog OnCreateDialog (Bundle bundle)
		{
			base.OnCreateDialog (bundle);

			catalogs = new CheckinShared.CatalogDB ();
			LayoutInflater inflater = Activity.LayoutInflater;
			View view = inflater.Inflate (Resource.Layout.AddCatalog, null);

			EditText textName = view.FindViewById<EditText> (Resource.Id.txtAgregarCatalogo);

			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.SetTitle ("Crear catálogo");
			builder.SetView (view);
			builder.SetPositiveButton ("Crear", delegate (object sender, DialogClickEventArgs e) {
				Catalog catalog = new Catalog ();

				if (textName.Text.Length != 0)
				{
					catalog.Name = textName.Text;
					catalog.Quantity = 0;
					catalog.UserId = AppHelper.GetCurrentUser(Activity).Id;
					catalogs.Insert(catalog);

					catalog.SaveToParse();

					Console.WriteLine("ParentFragment: " + ParentFragment);

					((CatalogsFragment) ((MainActivity) Activity).CurrentFragment()).AddCatalog(catalog);
				}
			});
			builder.SetNegativeButton ("Salir", delegate (object sender, DialogClickEventArgs e) { });


			return builder.Create ();
		}
	}
}

