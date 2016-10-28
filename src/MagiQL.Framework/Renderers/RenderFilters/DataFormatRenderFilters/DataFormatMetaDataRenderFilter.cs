using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Renderers.RenderFilters.DataFormatRenderFilters
{
    public abstract class DataFormatMetaDataRenderFilter : MetaDataRenderFilter
    { 
        protected abstract string DataFormatValue { get; }

        protected override string MetaDataKey
        {
            get { return "DataFormat"; }
        }

        public override bool CanApply(string value, ReportColumnMapping columnMapping, SearchResultRow row)
        {
            return base.CanApply(value, columnMapping, row) && columnMapping.MetaData.GetString(MetaDataKey) == DataFormatValue;
        }

    }
}