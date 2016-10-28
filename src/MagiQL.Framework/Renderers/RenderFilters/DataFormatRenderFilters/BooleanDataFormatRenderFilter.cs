using System;
using System.Data;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Renderers.RenderFilters.DataFormatRenderFilters
{
    public class BooleanDataFormatRenderFilter : DataFormatMetaDataRenderFilter
    {
        protected override string DataFormatValue
        {
            get { return "Boolean"; }
        }

        public override bool CanApply(string value, ReportColumnMapping columnMapping, SearchResultRow row)
        {
            return base.CanApply(value, columnMapping, row) || columnMapping.DbType == DbType.Boolean;
        }

        protected override string TryFormatValue( string value, ReportColumnMapping columnMapping, SearchResultRow row)
        {
            bool result;
            if (Boolean.TryParse(value, out result))
            {
                return result.ToString();
            }
            if (value == "1")
            {
                return true.ToString();
            }
            if (value == "0" || value == null)
            {
                return false.ToString();
            }

            return value;
        }

    }
}
