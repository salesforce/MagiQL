using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Renderers.RenderFilters.DataFormatRenderFilters
{
    public class PercentageDataFormatRenderFilter : DataFormatMetaDataRenderFilter
    {
        protected override string DataFormatValue
        {
            get { return "Percentage"; }
        }

        protected override string TryFormatValue(string value, ReportColumnMapping columnMapping, SearchResultRow row)
        { 
            return value;
        }

    }
}
