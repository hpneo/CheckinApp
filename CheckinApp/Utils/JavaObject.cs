using System;

namespace CheckinAppAndroid
{
	public class JavaObject<T> : Java.Lang.Object {
		public readonly T Value;

		public JavaObject (T value)
		{
			this.Value = value;
		}
	}
}

