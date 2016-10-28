using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Interfaces.Services
{
    public interface IReportColumnMappingQueryService
    {
        IList<ReportColumnMapping> GetReportColumnMappingsByOrganizationId(int dataSourceTypeId, int? organizationId);

        IList<ReportColumnMapping> GetReportColumnMappingsById(int dataSourceTypeId, IEnumerable<int> columnIds);
        IList<ReportColumnMapping> Find(int dataSourceId, string table, string field, int? actionSpecId, bool cacheOnly = false);
        IList<ReportColumnMapping> Find(int dataSourceTypeId, string uniqueName, bool cacheOnly = true);
        void ClearCache();
    }
}