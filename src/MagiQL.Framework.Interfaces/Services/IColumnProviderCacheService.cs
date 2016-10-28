using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Interfaces.Services
{
    public interface IColumnProviderCacheService
    {
        void SetMappings(int dataSourceTypeId, List<ReportColumnMapping> mappings);
        List<ReportColumnMapping> GetMappingsByOrganization(int dataSourceTypeId, int? organizationId);
        List<ReportColumnMapping> GetMappingById(int dataSourceTypeId, IEnumerable<int> columnIds);
        void ClearMappings(int dataSourceTypeId, int? organizationId);
        void ClearAllMappings();
        List<ReportColumnMapping> Find(int dataSourceTypeId, string table, string field, int? actionSpecId);
        List<ReportColumnMapping> Find(int dataSourceTypeId, string uniqueName);
        bool IsLazyLoaded(ReportColumnMapping reportColumnMapping);
    }
}