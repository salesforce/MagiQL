using MagiQL.Framework.Interfaces.Renderers;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Renderers.RenderFilters
{
    public abstract class MetaDataRenderFilter : IRenderFilter
    {
        protected abstract string MetaDataKey { get; }
        
        protected abstract string TryFormatValue(string value, ReportColumnMapping columnMapping, SearchResultRow row);

        public virtual bool CanApply(string value, ReportColumnMapping columnMapping, SearchResultRow row)
        {
            return value != null 
                   && columnMapping.MetaData != null
                   && columnMapping.MetaData.ContainsKey(MetaDataKey);
        }

        public virtual string ApplyFilter(string value, ReportColumnMapping columnMapping, SearchResultRow row)
        {
            if(CanApply(value,columnMapping,row))
            {
                return TryFormatValue(value, columnMapping, row);
            }

            return value;
        }

    }
}