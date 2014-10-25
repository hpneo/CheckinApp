
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

using SQLite;

using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using CheckinShared;
using CheckinShared.Models;
using CheckinShared.Services;
using Android.Views.Animations;

namespace CheckinAppAndroid
{
	[Activity (Label = "Catalog", Icon = "@drawable/icon", Theme="@android:style/Theme.Holo.Light")]			
	public class ListCatalogActivity : Activity
	{
		private CheckinShared.CatalogDB catalogs;
		private CatalogAdapter adapter;
		private ListView listViewCatalogs;

		public CatalogAdapter Adapter { get { return adapter; } }
		public ListView ListView { get { return listViewCatalogs; } }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			catalogs = new CheckinShared.CatalogDB ();

			SetContentView (Resource.Layout.ListCatalog);

			this.SetProgressBarIndeterminateVisibility (false);
			this.SetProgressBarVisibility (false);

			ProgressBar progressbarSearch = FindViewById<ProgressBar> (Resource.Id.CatalogListprogressBar);
			progressbarSearch.Visibility = ViewStates.Gone;

			listViewCatalogs = FindViewById<ListView> (Resource.Id.CatalogListView);
			adapter = new CatalogAdapter (this);

			SearchView searchViewCatalog = FindViewById<SearchView> (Resource.Id.CatalogListsearchView1);

			listViewCatalogs.Adapter = adapter;

			Toast.MakeText(this, catalogs.Count() + " catálogos creados", ToastLength.Long).Show();

			ActualizarLista ();

			listViewCatalogs.ItemLongClick += delegate(object sender, AdapterView.ItemLongClickEventArgs e) {
				DeleteCatalogDialogFragment dialog = new DeleteCatalogDialogFragment();
				dialog.Catalog = adapter.GetCatalog(e.Position);
				dialog.Show(FragmentManager, "DeleteCatalogDialogFragment");
				//				AlertDialog.Builder builder = new AlertDialog.Builder(this);//
				//				AlertDialog alertDialog = builder.Create();
				//				alertDialog.SetView(new TextView(context));//
				//				alertDialog.Show();
			};

			listViewCatalogs.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
				Intent intent = new Intent (this, typeof(MoviexCatalogActivity));
				intent.PutExtra("Name",adapter.GetCatalog(e.Position).Name);
				intent.PutExtra("Id",adapter.GetCatalog(e.Position).Id);
				StartActivityForResult (intent, 18);
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

			ActionBar.SetDisplayHomeAsUpEnabled (true);
			ActionBar.SetDisplayShowTitleEnabled (true);
			ActionBar.SetDisplayShowHomeEnabled (true);
		}

		public override bool OnCreateOptionsMenu(IMenu menu) {
			MenuInflater.Inflate (Resource.Menu.Main, menu);
			// ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			// "Creación de Tabs"

			var addMenu = menu.Add (0, 1, 1, "Add");
			addMenu.SetIcon (Resource.Drawable.Add);
			addMenu.SetShowAsAction (ShowAsAction.IfRoom);

			var refreshMenu = menu.Add (0, 2, 2, "Refresh");
			refreshMenu.SetIcon (Resource.Drawable.Refresh);
			refreshMenu.SetShowAsAction (ShowAsAction.IfRoom);

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item) {
			if (item.ItemId == 1) {
				AddCatalogDialogFragment dialog = new AddCatalogDialogFragment ();
				dialog.Show (FragmentManager, "AddCatalog");
			} else if (item.ItemId == 2) {
				Animation rotation = AnimationUtils.LoadAnimation (this, Resource.Animation.Rotate);

				rotation.RepeatCount = Animation.Infinite;

				ImageView imageView = (ImageView)LayoutInflater.Inflate (Resource.Layout.RefreshImageView, null);
				imageView.StartAnimation (rotation);

				item.SetActionView (imageView);

				ActualizarLista ();

				Handler handler = new Handler ();
				handler.PostDelayed (() => {
					imageView.ClearAnimation ();
					item.SetActionView (null);
				}, 1000);
			} else if (item.ItemId == 3) {
				Intent intent = new Intent (this, typeof(AuthActivity));
				StartActivityForResult (intent, 13);
			} else if (item.ItemId == Android.Resource.Id.Home) {
				OnBackPressed ();
			}
			return base.OnOptionsItemSelected (item);
		}

		public void ActualizarLista()
		{
			adapter.Clear ();
			foreach (Catalog catalog in catalogs.All()) {
				adapter.Add (catalog);
			}
		}

	}
}

