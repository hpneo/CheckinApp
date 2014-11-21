
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

namespace CheckinAppAndroid
{
	[Activity (Label = "Login", Theme = "@android:style/Theme.Light.NoTitleBar")]
	public class LoginActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var sharedPreferences = BaseContext.GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);

			int user_id = sharedPreferences.GetInt ("user_id", 0);

			Console.WriteLine ("LoginActivity:user_id: " + user_id);
			Toast.MakeText (this, "LoginActivity:user_id: " + user_id, ToastLength.Long).Show ();

			SetContentView (Resource.Layout.Login);

			Button buttonLoginWithTwitter = FindViewById<Button> (Resource.Id.buttonLoginWithTwitter);
			Button buttonLoginWithFacebook = FindViewById<Button> (Resource.Id.buttonLoginWithFacebook);

			var self = this;

			buttonLoginWithTwitter.Click += (object sender, EventArgs e) => {
				Intent intent = new Intent (self, typeof(AuthActivity));
				intent.PutExtra ("callerActivity", "LoginActivity");
				intent.PutExtra ("authService", "Twitter");
				self.StartActivityForResult (intent, (int)RequestsConstants.AuthRequest);
			};

			buttonLoginWithFacebook.Click += (object sender, EventArgs e) => {
				Intent intent = new Intent (self, typeof(AuthActivity));
				intent.PutExtra ("callerActivity", "LoginActivity");
				intent.PutExtra ("authService", "Facebook");
				self.StartActivityForResult (intent, (int)RequestsConstants.AuthRequest);
			};
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent intent)
		{
			Console.WriteLine ("OnActivityResult: " + requestCode);
			var sharedPreferences = BaseContext.GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);

			int user_id = sharedPreferences.GetInt ("user_id", 0);

			Console.WriteLine ("LoginActivity:user_id: " + user_id);
			Toast.MakeText (this, "LoginActivity:user_id: " + user_id, ToastLength.Long).Show ();

			if (requestCode == (int)RequestsConstants.AuthRequest) {
				if (resultCode == Result.Ok && user_id != 0) {
					Intent intentToMain = new Intent ();
					SetResult (Result.Ok, intentToMain);
					Finish ();
				}
			}
		}
	}
}

