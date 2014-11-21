using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;

using CheckinShared;
using CheckinShared.Models;

namespace CheckinAppAndroid
{
	public class AppHelper
	{
		public AppHelper ()
		{
		}

		public static User GetCurrentUser (Activity activity)
		{
			var sharedPreferences = activity.BaseContext.GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);

			int user_id = sharedPreferences.GetInt ("user_id", 0);

			if (user_id == 0) {
				return null;
			} else {
				UserDB users = new UserDB ();
				return users.Get (user_id);
			}
		}
	}
}

