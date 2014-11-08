using System;
using System.Collections.Generic;

using Android.Support.V4.App;

namespace CheckinAppAndroid
{
	public class AppPagerAdapter : FragmentPagerAdapter
	{
		private List<Android.Support.V4.App.Fragment> fragments = new List<Android.Support.V4.App.Fragment>();

		public override int Count
		{
			get { return fragments.Count; }
		}

		public AppPagerAdapter (FragmentManager fragmentManager) : base(fragmentManager)
		{
			fragments.Add (new CheckinsFragment ());
			fragments.Add (new CatalogsFragment ());
		}

		public void AddFragment(Fragment fragment) {
			fragments.Add (fragment);
		}

		public override Android.Support.V4.App.Fragment GetItem(int position) {
			Android.Support.V4.App.Fragment fragment = fragments [position];

			return fragment;
		}

		public void RefreshList(int position) {
			if (position == 0) {
				((CheckinsFragment)fragments [0]).RefreshList ();
			} else {
				((CatalogsFragment)fragments [1]).RefreshList ();
			}
		}
	}
}

