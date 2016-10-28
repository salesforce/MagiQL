using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces.Services
{
    public interface IReportStatusQueryService
    {
        ReportStatus GetReportStatus(long id);
    }
}