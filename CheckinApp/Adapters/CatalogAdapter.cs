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
	public class CatalogAdapter : BaseAdapter
	{
		private ObservableCollection<Catalog> catalogsList;
		private Activity activity;

		// private readonly object locker = new object();

		public CatalogAdapter (Activity activity) {
			this.activity = activity;
			this.catalogsList = new ObservableCollection<Catalog> ();
			this.catalogsList.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e) {
				base.NotifyDataSetChanged();
				// NotifyDataSetChanged();
			};
		}

		public override int Count {
			get { return catalogsList.Count; }
		}

		public void Clear() {
			catalogsList.Clear ();
		}

		public void Add(Catalog catalog) {
			catalogsList.Add (catalog);
		}

		public void AddAll(IList<Catalog> catalogs) {
			catalogsList.Clear ();
			foreach (Catalog catalog in catalogs) {
				catalogsList.Add (catalog);
			}
		}

		public void Insert(Catalog catalog, int position) {
			catalogsList.Insert (position, catalog);
		}

		public void Remove(Catalog catalog) {
			catalogsList.Remove (catalog);
		}

		public override Java.Lang.Object GetItem (int position) {
			return new JavaObject<CheckinShared.Models.Catalog> (catalogsList [position]);
		}

		public Catalog GetCatalog (int position) {
			return (Catalog) catalogsList [position];
		}

		public override long GetItemId (int position) {
			return (catalogsList [position]).Id;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView;
			ViewHolder holder;

			if (view != null) {
				holder = view.Tag as ViewHolder;
			} else {
				holder = new ViewHolder ();
				view = activity.LayoutInflater.Inflate (Resource.Layout.ItemCatalog, parent, false);
				holder.Name = view.FindViewById<TextView> (Resource.Id.textViewItemCatalog);
				holder.NumberElements = view.FindViewById<TextView> (Resource.Id.textViewItemNumber);
				view.Tag = holder;
			}

			Catalog catalog = ((Catalog)catalogsList [position]);

			if (catalog != null) {
				holder.Name.Text = catalog.Name;
				holder.NumberElements.Text = catalog.Quantity + " peliculas";
			}

			return view;
		}

		private class ViewHolder : Java.Lang.Object {
			public TextView Name { get; set; }
			public TextView NumberElements { get; set; }
		}
	}
}

