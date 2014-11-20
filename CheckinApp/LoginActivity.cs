
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
	[Activity (Label = "Login", Theme="@android:style/Theme.Light.NoTitleBar")]
	public class LoginActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Login);

			Button buttonLoginWithTwitter = (Button)FindViewById (Resource.Id.buttonLoginWithTwitter);
			Button buttonLoginWithFacebook = (Button)FindViewById (Resource.Id.buttonLoginWithFacebook);

			buttonLoginWithTwitter.Touch += (object sender, View.TouchEventArgs e) => {
				Intent intent = new Intent (this, typeof(AuthActivity));
				intent.PutExtra ("authService", "Twitter");
				StartActivityForResult (intent, (int) RequestsConstants.AuthRequest);
			};

			buttonLoginWithFacebook.Touch += (object sender, View.TouchEventArgs e) => {
				Intent intent = new Intent (this, typeof(AuthActivity));
				intent.PutExtra ("authService", "Facebook");
				StartActivityForResult (intent, (int) RequestsConstants.AuthRequest);
			};
		}

		async protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent) {
			if (requestCode == (int)RequestsConstants.AuthRequest) {
				if (resultCode == Result.Ok) {
					StartActivity (typeof(MainActivity));
					Finish ();
				}
			}
		}
	}
}

