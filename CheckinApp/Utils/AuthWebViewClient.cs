using System;

using Android.Content;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;

using CheckinShared;
using CheckinShared.Models;

namespace CheckinAppAndroid
{
	public class AuthWebViewClient : WebViewClient
	{
		private AuthActivity activity;

		public AuthWebViewClient (AuthActivity activity)
		{
			this.activity = activity;
		}

		public override bool ShouldOverrideUrlLoading (WebView view, string url)
		{
			view.LoadUrl (url);
			return true;
		}

		public override void OnPageFinished (WebView webView, string url)
		{
			if (url.Contains ("http://canchitapp.herokuapp.com/info")) {
				Android.Net.Uri uri = Android.Net.Uri.Parse (url);
				Console.WriteLine (uri.GetQueryParameter ("token"));

				var sharedPreferences = activity.GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);
				var editor = sharedPreferences.Edit ();
		
				string uid = uri.GetQueryParameter ("uid");
				string token = uri.GetQueryParameter ("token");
				string secret = uri.GetQueryParameter ("secret");

				UserDB userDB = new UserDB ();
				User user = new User ();

				int count;

				int user_id = sharedPreferences.GetInt ("user_id", 0);

				if (user_id == 0) {
					if (this.activity.AuthService == "facebook") {
						count = userDB.All ().Where (u => u.Facebook.Equals (uid)).Count ();

						if (count == 0) {
							user.Facebook = uid;
						}
					} else if (this.activity.AuthService == "twitter") {
						count = userDB.All ().Where (u => u.Twitter.Equals (uid)).Count ();

						if (count == 0) {
							user.Twitter = uid;
						}
					}

					userDB.Insert (user);
				} else {
					user = userDB.Get (user_id);

					if (this.activity.AuthService == "facebook") {
						user.Facebook = uid;
					} else if (this.activity.AuthService == "twitter") {
						user.Twitter = uid;
					}

					userDB.Update (user);
				}

				editor.PutInt ("user_id", user.Id);

				user.SaveToParse ();

				editor.PutString (this.activity.AuthService + ":token", token);
				editor.PutString (this.activity.AuthService + ":secret", secret);

				editor.Commit ();

				Intent intent = new Intent ();

				intent.PutExtra ("authService", this.activity.AuthService);
				intent.PutExtra ("token", token);
				intent.PutExtra ("secret", secret);

				activity.SetResult (Result.Ok, intent);
				activity.Finish ();
			}

			base.OnPageFinished (webView, url);
		}
	}
}

