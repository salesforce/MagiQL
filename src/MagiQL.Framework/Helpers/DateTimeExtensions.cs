using System;

namespace MagiQL.Framework.Helpers
{
	public static class DateTimeExtensions
	{
	    public static long ToUnixTime(this DateTime utc)
		{
			return UnixTimeHelpers.ToUnixTime(utc);
		}

        public static DateTime DateTimeFromUnixTime(this double unixTime)
        {
            return UnixTimeHelpers.FromUnixTime(unixTime);
        }

        public static DateTime DateTimeFromUnixTime(this long unixTime)
        {
            return UnixTimeHelpers.FromUnixTime(unixTime);
        }
	}
}
