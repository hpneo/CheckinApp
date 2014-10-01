using System;

using Android.Webkit;
using Android.Views;

namespace CheckinAppAndroid
{
	public class AuthWebViewClient : WebViewClient
	{
		public AuthWebViewClient ()
		{
		}

		public override bool ShouldOverrideUrlLoading (WebView view, string url)
		{
			view.LoadUrl (url);
			return true;
		}

		public override void OnPageFinished(WebView webView, string url) {
			if (url.Contains ("http://checkinapp-auth.herokuapp.com/auth/facebook/callback")) {
				webView.Visibility = Android.Views.ViewStates.Gone;
			}

			base.OnPageFinished(webView, url);
		}
	}
}

