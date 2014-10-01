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

using Android.Webkit;

namespace CheckinAppAndroid
{
	[Activity (Label = "CheckinApp", Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar")]
	public class AuthActivity : Activity
	{
		private WebView webView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.AuthDialog);

			WebView webView = FindViewById<WebView> (Resource.Id.webviewDialog);

			webView.Settings.JavaScriptEnabled = true;
			webView.Settings.SavePassword = false;
			webView.Settings.SaveFormData = false;

			webView.RequestFocus (FocusSearchDirection.Down);
			/*webView.Touch += (object sender, View.TouchEventArgs e) => {
				switch (e.Event.Action) {
				case MotionEventActions.Up:
					if (!webView.HasFocus) {
						webView.RequestFocus();
					}
					break;
				case MotionEventActions.Down:
					if (!webView.HasFocus) {
						webView.RequestFocus();
					}
					break;
				}
			};*/

			webView.LoadUrl ("http://checkinapp-auth.herokuapp.com/auth/twitter");

			webView.SetWebViewClient (new AuthWebViewClient ());
		}

		public override bool OnKeyDown (Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
		{
			if (keyCode == Keycode.Back && webView.CanGoBack ()) {
				webView.GoBack ();
				return true;
			}

			return base.OnKeyDown (keyCode, e);
		}
	}
}

