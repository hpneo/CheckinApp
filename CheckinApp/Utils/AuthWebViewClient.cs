using System;

using Android.Content;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Runtime;
using Android.Widget;

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
			base.OnPageFinished (webView, url);

			Android.Net.Uri uri = Android.Net.Uri.Parse (url);

			Console.WriteLine ("uri.Path: " + uri.Path);

			if (uri.Path.StartsWith("/info")) {
				Console.WriteLine (uri.GetQueryParameter ("token"));

				var sharedPreferences = activity.BaseContext.GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);
				var editor = sharedPreferences.Edit ();
		
				string uid = uri.GetQueryParameter ("uid");
				string token = uri.GetQueryParameter ("token");
				string secret = uri.GetQueryParameter ("secret");

				UserDB userDB = new UserDB ();
				User user = new User ();

				int count;

				int user_id = sharedPreferences.GetInt ("user_id", 0);

				if (user_id == 0) {
					Console.WriteLine ("user_id == 0");
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
					Console.WriteLine ("user_id != 0");
					user = userDB.Get (user_id);

					if (this.activity.AuthService == "facebook") {
						user.Facebook = uid;
					} else if (this.activity.AuthService == "twitter") {
						user.Twitter = uid;
					}

					userDB.Update (user);
				}

				editor.PutInt ("user_id", user.Id);

				editor.PutString (this.activity.AuthService + ":token", token);
				editor.PutString (this.activity.AuthService + ":secret", secret);
				Console.WriteLine ("AuthWebViewClient:user_id: " + user.Id);

				editor.Commit ();

				Toast.MakeText (activity, "AuthWebViewClient:user_id: " + user_id, ToastLength.Short).Show ();

				Intent intentToLogin = new Intent ();

				intentToLogin.PutExtra ("authService", activity.AuthService);
				intentToLogin.PutExtra ("token", token);
				intentToLogin.PutExtra ("secret", secret);

				activity.SetResult (Result.Ok, intentToLogin);
				activity.Finish ();

				Console.WriteLine ("Finishing: " + activity.Intent.GetStringExtra("callerActivity"));
			}
		}
	}
}

