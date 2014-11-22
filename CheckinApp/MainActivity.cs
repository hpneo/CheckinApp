using System;
using System.IO;
using System.Collections;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;

using Android.Support.V4.View;
using Android.Support.V4.App;

using Android.Webkit;

using SQLite;

using CheckinShared.Models;
using Android.Provider;
using Android.Content.PM;
using System.Collections.Generic;

namespace CheckinAppAndroid
{
	[Activity (Label = "CanchitApp", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Holo.Light")]
	public class MainActivity : Android.Support.V4.App.FragmentActivity
	{
		private ViewPager appViewPager;
		private AppPagerAdapter appPagerAdapter;

		int category;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var sharedPreferences = BaseContext.GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);

			int user_id = sharedPreferences.GetInt ("user_id", 0);

			Console.WriteLine ("MainActivity:user_id: " + user_id);

			if (user_id == 0) {
				StartActivityForResult (typeof(LoginActivity), (int)RequestsConstants.LoginRequest);
			} else {
				LoadMainActivity (bundle);
			}
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent intent)
		{
			if (requestCode == (int)RequestsConstants.LoginRequest) {
				if (resultCode == Result.Ok) {
					LoadMainActivity (null);
				}
			} else {
				if (appViewPager.CurrentItem == 0) {
					if (resultCode == Result.Ok) {
						int movieId = intent.GetIntExtra ("movieId", 0);
						((CheckinsFragment)appPagerAdapter.GetItem (0)).AddMovie (movieId);
					}
				}
			}
		}


		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.Main, menu);

			/*var searchMenu = menu.Add ("Search");
			searchMenu.SetVisible (true);
			searchMenu.SetShowAsAction (ShowAsAction.IfRoom);
			searchMenu.SetActionView(new SearchView(this));*/

			var addMenu = menu.Add (1, (int)MenuConstants.MainAdd, 1, "Add");
			addMenu.SetIcon (Resource.Drawable.Add);
			addMenu.SetShowAsAction (ShowAsAction.IfRoom);

			var refreshMenu = menu.Add (1, (int)MenuConstants.MainRefresh, 2, "Refresh");
			refreshMenu.SetIcon (Resource.Drawable.Refresh);
			refreshMenu.SetShowAsAction (ShowAsAction.IfRoom);

			/*var facebookMenu = menu.Add (2, (int)MenuConstants.MainFacebook, 3, "Authorize Facebook");
			facebookMenu.SetIcon (Resource.Drawable.Facebook);
			facebookMenu.SetShowAsAction (ShowAsAction.IfRoom);

			var twitterMenu = menu.Add (2, (int)MenuConstants.MainTwitter, 4, "Authorize Twitter");
			twitterMenu.SetIcon (Resource.Drawable.Twitter);
			twitterMenu.SetShowAsAction (ShowAsAction.IfRoom);*/

			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			Intent intent;
			switch (item.ItemId) {
			case (int) MenuConstants.MainAdd:
				AddItem (item);
				break;
			case (int) MenuConstants.MainRefresh:
				RefreshItems (item);
				break;
			case (int) MenuConstants.MainFacebook:
				intent = new Intent (this, typeof(AuthActivity));
				intent.PutExtra ("authService", "Facebook");
				StartActivityForResult (intent, (int)RequestsConstants.AuthRequest);
				break;
			case (int) MenuConstants.MainTwitter:
				intent = new Intent (this, typeof(AuthActivity));
				intent.PutExtra ("authService", "Twitter");
				StartActivityForResult (intent, (int)RequestsConstants.AuthRequest);
				break;
			}

			return base.OnOptionsItemSelected (item);
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			if (appViewPager != null && appViewPager.CurrentItem == 1) {
				appPagerAdapter.RefreshList (appViewPager.CurrentItem);
			}
		}

		void LoadMainActivity (Bundle bundle)
		{
			Console.WriteLine (AppHelper.GetCurrentUser (this));
			SetContentView (Resource.Layout.Main);

			appViewPager = FindViewById<ViewPager> (Resource.Id.appViewPager);

			ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			appPagerAdapter = new AppPagerAdapter (SupportFragmentManager);

			appViewPager.Adapter = appPagerAdapter;
			appViewPager.SetOnPageChangeListener (new ViewPageListenerForActionBar (ActionBar));

			var tabMovies = ActionBar.NewTab ();
			tabMovies.SetText ("Películas");
			tabMovies.TabSelected += (object sender, ActionBar.TabEventArgs e) => {
				appViewPager.CurrentItem = ActionBar.SelectedNavigationIndex;
			};

			ActionBar.AddTab (tabMovies);

			var tabPopular = ActionBar.NewTab ();
			tabPopular.SetText ("Catálogo");
			tabPopular.TabSelected += (object sender, ActionBar.TabEventArgs e) => {
				appViewPager.CurrentItem = ActionBar.SelectedNavigationIndex;
			};

			ActionBar.AddTab (tabPopular);

			if (bundle != null) {
				category = bundle.GetInt ("Películas");
				ActionBar.SelectTab (ActionBar.GetTabAt (category));
			}
		}

		void AddItem (IMenuItem item)
		{
			switch (appViewPager.CurrentItem) {
			case 0:
				Intent intent = new Intent (this, typeof(AddMovieActivity));
				((CheckinsFragment)CurrentFragment ()).StartActivityForResult (intent, (int)RequestsConstants.AddMovieRequest);
				break;
			case 1:
				AddCatalogDialogFragment dialog = new AddCatalogDialogFragment ();
				dialog.Show (SupportFragmentManager, "AddCatalog");
				break;
			}
		}

		void RefreshItems (IMenuItem item)
		{
			ImageView imageView;
			
			Animation rotation = AnimationUtils.LoadAnimation (this, Resource.Animation.Rotate);
			rotation.RepeatCount = Animation.Infinite;

			imageView = (ImageView)LayoutInflater.Inflate (Resource.Layout.RefreshImageView, null);
			imageView.StartAnimation (rotation);

			item.SetActionView (imageView);
			

			appPagerAdapter.RefreshList (appViewPager.CurrentItem);

			
			Handler handler = new Handler ();
			handler.PostDelayed (() => {
				imageView.ClearAnimation ();
				item.SetActionView (null);
			}, 1000);
			
		}

		public Android.Support.V4.App.Fragment CurrentFragment ()
		{
			return appPagerAdapter.GetItem (appViewPager.CurrentItem);
		}
	}

	public class ViewPageListenerForActionBar : ViewPager.SimpleOnPageChangeListener
	{
		private ActionBar actionBar;

		public ViewPageListenerForActionBar (ActionBar bar)
		{
			actionBar = bar;
		}

		public override void OnPageSelected (int position)
		{
			actionBar.SetSelectedNavigationItem (position);
		}
	}
}


