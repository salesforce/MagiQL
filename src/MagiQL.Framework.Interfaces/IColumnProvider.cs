using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces
{
    public interface IColumnProvider
    {   
        ReportColumnMapping GetColumnMapping(int dataSourceId, int id);
        List<ReportColumnMapping> GetColumnMappings(int dataSourceId, IEnumerable<int> columnIds);
        List<ReportColumnMapping> GetAllColumnMappings(int dataSourceId, int? organizationId);
        List<ReportColumnMapping> Find(int dataSourceId, string table, string field, int? statTransposeKeyValue, bool cacheOnly = false);
        List<ReportColumnMapping> Find(int dataSourceId, string uniqueName, bool cacheOnly = true);
        List<ColumnDefinition> GetAllSelectableColumnDefinitions(int dataSourceId, int? organizationId, int? groupBy = null); 

        ReportColumnMapping UpdateColumnMapping(int dataSourceId, int columnId, ReportColumnMapping columnMapping, IReportColumnMappingValidator columnValidator);
        ReportColumnMapping CreateColumnMapping(int dataSourceId, ReportColumnMapping columnMapping, IReportColumnMappingValidator columnValidator);
        void ClearCache(int dataSourceId);

        void Initialize();
    }
}
