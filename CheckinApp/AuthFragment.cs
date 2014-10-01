
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Android.Webkit;

namespace CheckinAppAndroid
{
	public class AuthFragment : DialogFragment
	{
		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			base.OnCreateDialog (savedInstanceState);

			LayoutInflater inflater = Activity.LayoutInflater;

			View view = inflater.Inflate (Resource.Layout.AuthDialog, null);
			WebView dialogWebView = newWebView (view);

			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.SetView (view);

			return builder.Create ();
		}

		private WebView newWebView(View view) {
			WebView webView = view.FindViewById<WebView> (Resource.Id.webviewDialog);

			webView.Focusable = true;

			webView.FocusableInTouchMode = true;
			webView.Clickable = true;

			webView.Settings.AllowContentAccess = true;
			webView.Settings.SetNeedInitialFocus (true);
			webView.Settings.JavaScriptEnabled = true;
			webView.Settings.SavePassword = false;
			webView.Settings.SaveFormData = false;
			webView.Settings.LoadWithOverviewMode = true;
			webView.Settings.UseWideViewPort = true;

			webView.RequestFocus (FocusSearchDirection.Down);
			webView.Touch += (object sender, View.TouchEventArgs e) => {
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
			};

			webView.LoadUrl ("http://checkinapp-auth.herokuapp.com/auth/twitter");

			webView.SetWebViewClient (new AuthWebViewClient ());
			webView.RequestFocus ();

			return webView;
		}
	}
}