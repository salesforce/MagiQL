using System;

namespace MagiQL.Framework.Helpers
{
	public static class UnixTimeHelpers
	{
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Converts the provided UTC date/time value to UNIX time (seconds
		/// since Unix Epoch).
		/// </summary>
		public static long ToUnixTime(DateTime utcDate)
		{
		    if (utcDate.Kind != DateTimeKind.Utc)
		    {
		        throw new ArgumentException(String.Format("Invalid DateTime value provided. Expected to receive a UTC DateTime but got a DateTime value with a Kind of [{0}] instead.", utcDate.Kind));
		    }

            return (long)((utcDate - UnixEpoch).TotalSeconds);
		}

        /// <summary>
        /// Converts the provided UNIX time (seconds since Unix Epoch) to a UTC 
        /// DateTime value.
        /// </summary>
        public static DateTime FromUnixTime(long unixTime)
        {
            return UnixEpoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// Converts the provided UNIX time (seconds since Unix Epoch) to a UTC 
        /// DateTime value.
        /// </summary>
        public static DateTime FromUnixTime(double unixTime)
        {
            return UnixEpoch.AddSeconds(unixTime);
        }
	}
}
