using System.Data;
using DapperExtensions;
using MagiQL.Framework.Interfaces.Repository;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Repositories.Repositories
{
    public class ReportStatusRepository : ReportsRepository<ReportStatus>, IReportStatusRepository
    {
        public ReportStatus GetReportStatus(long id)
        {
            return Get(id);
        }

        public void Delete(int id)
        {
            var status = GetReportStatus(id);
            using (var scope = CreateTransaction())
            {
                Delete(status, scope);
                scope.Commit();
            }
        }

        public void Update(ReportStatus existing, IDbTransaction scope)
        {
            GetConnection(scope).Update(existing, scope);
        }
    }
}