using System.Collections.Generic;
using System.Linq;

namespace Scenarios.Scenario1.Tests.Integration.Helpers
{
    public static class ListExtensions
    {
        public static List<string> ToDecimalString(this List<string> source)
        {
            return source.Select(RemoveDecimalZeros).ToList();
        }

        public static string RemoveDecimalZeros(decimal val)
        {
            var result = val.ToString();
            if (result.Contains("."))
            {
                result = result.TrimEnd('0');
            }
            return result;
        }

        public static string RemoveDecimalZeros(string val)
        {
            decimal d;
            if (decimal.TryParse(val, out d))
            {
                return RemoveDecimalZeros(d);
            }
            return val;
        }
    }
}