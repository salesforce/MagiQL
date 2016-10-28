using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Services
{
    public abstract class ColumnProviderBase : IColumnProvider
    {
        private static List<string> _initializedProviders = new List<string>();
          
        public abstract ReportColumnMapping GetColumnMapping(int dataSourceId, int id);

        public abstract List<ReportColumnMapping> GetColumnMappings(int dataSourceId, IEnumerable<int> columnIds);

        public abstract List<ReportColumnMapping> GetAllColumnMappings(int dataSourceId, int? organizationId);

        public abstract List<ReportColumnMapping> Find(int dataSourceId, string table, string field, int? statTransposeKeyValue, bool cacheOnly = false);

        public abstract List<ReportColumnMapping> Find(int dataSourceId, string uniqueName, bool cacheOnly = true);

        public virtual List<ColumnDefinition> GetAllSelectableColumnDefinitions(int dataSourceId, int? organizationId, int? groupBy = null)
        {
            ReportColumnMapping groupByColumn = null;
            if (groupBy != null)
            {
                groupByColumn = GetColumnMappings(dataSourceId, new[] {groupBy.Value}).Single();
            }

            return GetAllColumnMappings(dataSourceId, organizationId).Where(x => IsSelectable(x, groupByColumn)).Select(x=> new ColumnDefinition(x)).ToList();
        }

        public abstract ReportColumnMapping UpdateColumnMapping(int dataSourceId, int columnId, ReportColumnMapping columnMapping, IReportColumnMappingValidator columnValidator);

        public abstract ReportColumnMapping CreateColumnMapping(int dataSourceId, ReportColumnMapping columnMapping, IReportColumnMappingValidator columnValidator);
        
        public abstract void ClearCache(int dataSourceId);

        public void Initialize()
        {  
            if (!_initializedProviders.Contains(this.GetType().FullName))
            {
                // code to be executed the first time this column provider is loaded
                Start();
                _initializedProviders.Add(this.GetType().FullName);
            }
        }

        public virtual void Start(){}

        protected abstract bool IsSelectable(ReportColumnMapping column, ReportColumnMapping groupBy);

    }
}
