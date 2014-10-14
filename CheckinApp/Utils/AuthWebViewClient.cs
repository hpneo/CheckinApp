using System;

using Android.Content;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;

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

    public override void OnPageFinished(WebView webView, string url) {
      if (url.Contains ("http://checkinapp-auth.herokuapp.com/info")) {
        Android.Net.Uri uri = Android.Net.Uri.Parse (url);
        Console.WriteLine(uri.GetQueryParameter ("token"));

        var sharedPreferences = activity.GetSharedPreferences ("CheckinAppPreferences", FileCreationMode.WorldWriteable);
        var editor = sharedPreferences.Edit ();
        editor.PutString ("token", uri.GetQueryParameter ("token"));

        Intent intent = new Intent ();

        editor.Commit ();

        intent.PutExtra ("token", uri.GetQueryParameter ("token"));

        activity.SetResult (Result.Ok, intent);
        activity.Finish ();
      }

      base.OnPageFinished(webView, url);
    }
  }
}

