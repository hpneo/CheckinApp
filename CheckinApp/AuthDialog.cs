using System;

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
	public class AuthDialog : Dialog
	{
		public string Url { get; set; }
		public AuthDialog (Context context, string url) : base(context)
		{
			Url = url;
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			View view = LayoutInflater.Inflate (Resource.Layout.AuthDialog, null, false);
			WebView dialogWebView = newWebView (view);

			Display display = Window.WindowManager.DefaultDisplay;
			float scale = Context.Resources.DisplayMetrics.Density;
			Android.Content.Res.Orientation orientation = Context.Resources.Configuration.Orientation;
			float dimension_width = (orientation.Equals(Orientation.Horizontal)) ? 20 : 40 ;
			float dimension_height = 60;

			AddContentView(view, new ViewGroup.LayoutParams(
				(int) (display.Width - (dimension_width * scale + 0.5f)),
				(int) (display.Height - (dimension_height * scale + 0.5f))
			));
		}

		private WebView newWebView(View view) {
			WebView webView = view.FindViewById<WebView> (Resource.Id.webviewDialog);

			webView.Settings.JavaScriptEnabled = true;
			webView.Settings.SavePassword = false;
			webView.Settings.SaveFormData = false;

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

