using MagiQL.Framework.Helpers;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Renderers.RenderFilters.DataFormatRenderFilters
{
    public class UtcDateTimeDataFormatRenderFilter : DataFormatMetaDataRenderFilter
    {
        protected override string DataFormatValue
        {
            get { return "UtcDateTime"; }
        }

        protected override string TryFormatValue(string value, ReportColumnMapping columnMapping, SearchResultRow row)
        {
            long parsed;
            if (long.TryParse(value, out parsed))
            {
                if (parsed > 0)
                {
                    var date = parsed.DateTimeFromUnixTime();
                    return date.ToString("dd/MM/yyyy HH:mm:ss");
                }
            }
            return value;
        }

    }
}
