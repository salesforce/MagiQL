using System;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Renderers.RenderFilters
{
    /// <summary>
    /// Detects numerical values and rounds them to the specified number of decimal places
    /// </summary>
    public class PrecisionRenderFilter : MetaDataRenderFilter
    {
        protected override string MetaDataKey
        {
            get { return "Precision"; }
        }

        protected override string TryFormatValue( string value, ReportColumnMapping columnMapping, SearchResultRow row)
        {
            double parsed;
            if (value.Contains(".") && double.TryParse(value, out parsed))
            {
                var toPrecision = Math.Round(parsed, columnMapping.MetaData.GetInt(MetaDataKey));
                return toPrecision.ToString();
            } 

            return value;
        }
    }
}