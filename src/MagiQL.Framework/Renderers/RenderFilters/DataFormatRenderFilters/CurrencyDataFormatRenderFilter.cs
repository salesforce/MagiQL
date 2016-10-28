using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Renderers.RenderFilters.DataFormatRenderFilters
{
    public class CurrencyDataFormatRenderFilter : DataFormatMetaDataRenderFilter
    {
        protected override string DataFormatValue
        {
            get { return "Currency"; }
        }

        protected override string TryFormatValue( string value, ReportColumnMapping columnMapping, SearchResultRow row)
        {
            double parsed;
            if (double.TryParse(value, out parsed))
            {
                //return "?" + parsed.ToString("0.00");
                return parsed.ToString("0.00");
            }
            return value;
        }

    }
}
