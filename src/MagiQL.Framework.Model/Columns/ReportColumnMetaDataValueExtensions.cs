using System.Collections.Generic;
using System.Linq;

namespace MagiQL.Framework.Model.Columns
{


    public static class ReportColumnMetaDataValueExtensions
    {
        public static bool ContainsKey(this ICollection<ReportColumnMetaDataValue> metaData, string name)
        {
            return metaData != null && metaData.Any(x => x.Name == name);
        }

        public static string GetString(this ICollection<ReportColumnMetaDataValue> metaData, string name)
        {
            if (metaData == null || !metaData.ContainsKey(name))
            {
                return null;
            }

            return metaData.First(x => x.Name == name).Value;
        }

        public static bool GetBool(this ICollection<ReportColumnMetaDataValue> metaData, string name)
        {
            var valueString = metaData.GetString(name);
            bool result = false;
            if (!string.IsNullOrEmpty(valueString))
            {
                bool.TryParse(valueString, out result);
            }
            return result;
        }

        public static int GetInt(this ICollection<ReportColumnMetaDataValue> metaData, string name)
        {
            var valueString = metaData.GetString(name);
            int result = 0;
            if (!string.IsNullOrEmpty(valueString))
            {
                int.TryParse(valueString, out result);
            }
            return result;
        }

        public static long GetLong(this ICollection<ReportColumnMetaDataValue> metaData, string name)
        {
            var valueString = metaData.GetString(name);
            long result = 0;
            if (!string.IsNullOrEmpty(valueString))
            {
                long.TryParse(valueString, out result);
            }
            return result;
        }

        public static double GetDouble(this ICollection<ReportColumnMetaDataValue> metaData, string name)
        {
            var valueString = metaData.GetString(name);
            double result = 0;
            if (!string.IsNullOrEmpty(valueString))
            {
                double.TryParse(valueString, out result);
            }
            return result;
        }

    }
}
