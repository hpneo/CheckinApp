using System;

using Android.App;
using Android.Runtime;

using Parse;

namespace CheckinAppAndroid
{
	[Application]
	public class App : Application
	{
		public App (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnCreate ()
		{
			base.OnCreate ();
			ParseClient.Initialize ("EeUBYPSNfzYRiKVJdR49ZRCQvQIiEpZfNKa0qGUe", "OSlo58CZsG8MDFY6bWUB8bkh8akXxCdX3YKZkevt");
		}
	}
}

