using System.Data;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces.Repository
{
    public interface IReportStatusRepository : IRepository
    {
        ReportStatus GetReportStatus(long id);
        void Delete(int id);
        ReportStatus Add(ReportStatus campaign, IDbTransaction scope);
        void Update(ReportStatus existing, IDbTransaction scope);
    }
}