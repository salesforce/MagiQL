using System.Collections.Generic;
using System.Data;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Interfaces.Repository
{
    public interface IReportColumnMappingRepository : IRepository
    {
        ReportColumnMapping GetReportColumnMapping(int id);
        void Delete(int id);
        ReportColumnMapping Add(ReportColumnMapping campaign, IDbTransaction scope);

        IList<ReportColumnMapping> GetReportColumnMappingsByOrganizationId(int dataSourceTypeId, int? orgId);
        IList<ReportColumnMapping> GetReportColumnMappingsById(int dataSourceTypeId, IEnumerable<int> columnIds);
        IList<ReportColumnMapping> Find(int dataSourceTypeId, string table, string field, int? actionSpecId);
        IList<ReportColumnMapping> Find(int dataSourceTypeId, string uniqueName);
        void Update(ReportColumnMapping entity, IDbTransaction scope);
    }
}
