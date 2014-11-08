using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using CheckinShared;
using CheckinShared.Models;
using CheckinShared.Services;
using Android.Views.Animations;

namespace CheckinAppAndroid
{
	public class CatalogsFragment : Android.Support.V4.App.Fragment
	{
		private CheckinShared.CatalogDB catalogs;
		private CatalogAdapter adapter;
		private ListView listViewCatalogs;

		public CatalogAdapter Adapter { get { return adapter; } }
		public ListView ListView { get { return listViewCatalogs; } }

		public CatalogsFragment ()
		{
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View rootView = inflater.Inflate(Resource.Layout.CatalogsFragment, container, false);

			catalogs = new CheckinShared.CatalogDB ();

			Activity.SetProgressBarIndeterminateVisibility (false);
			Activity.SetProgressBarVisibility (false);

			ProgressBar progressbarSearch = rootView.FindViewById<ProgressBar> (Resource.Id.CatalogListprogressBar);
			progressbarSearch.Visibility = ViewStates.Gone;

			listViewCatalogs = rootView.FindViewById<ListView> (Resource.Id.CatalogListView);
			adapter = new CatalogAdapter (Activity);

			SearchView searchViewCatalog = rootView.FindViewById<SearchView> (Resource.Id.CatalogListsearchView1);

			listViewCatalogs.Adapter = adapter;

			Toast.MakeText(Activity, catalogs.Count() + " catálogos creados", ToastLength.Long).Show();

			listViewCatalogs.ItemLongClick += delegate(object sender, AdapterView.ItemLongClickEventArgs e) {
				DeleteCatalogDialogFragment dialog = new DeleteCatalogDialogFragment();
				dialog.Catalog = adapter.GetCatalog(e.Position);
				dialog.Show(FragmentManager, "DeleteCatalogDialogFragment");
			};

			listViewCatalogs.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
				Intent intent = new Intent (Activity, typeof(CatalogActivity));
				intent.PutExtra("Name",adapter.GetCatalog(e.Position).Name);
				intent.PutExtra("Id",adapter.GetCatalog(e.Position).Id);
				// StartActivityForResult (intent, 18);
				StartActivity(intent);
			};

			searchViewCatalog.QueryTextSubmit += delegate(object sender, SearchView.QueryTextSubmitEventArgs e) {
				Console.WriteLine(searchViewCatalog.Query);
				progressbarSearch.Visibility = ViewStates.Visible;
				adapter.Clear();
				foreach (Catalog catalog in catalogs.All()) {
					if(searchViewCatalog.Query.IndexOf(catalog.Name) != -1)
					{
						adapter.Add(catalog);
					}
				}
				progressbarSearch.Visibility = ViewStates.Gone;
			};

			searchViewCatalog.Close += delegate(object sender, SearchView.CloseEventArgs e) {
				progressbarSearch.Visibility = ViewStates.Gone;
			};

			RefreshList ();

			return rootView;
		}

		public void AddCatalog(int catalogId) {
			var catalog = catalogs.Get (catalogId);

			adapter.Add (catalog);
		}

		public void AddCatalog(Catalog catalog) {
			adapter.Add (catalog);
		}

		public void RemoveCatalog(Catalog catalog) {
			adapter.Remove (catalog);
		}

		public void RefreshList()
		{
			adapter.Clear ();

			foreach (Catalog catalog in catalogs.All()) {
				adapter.Add (catalog);
			}
		}
	}
}

