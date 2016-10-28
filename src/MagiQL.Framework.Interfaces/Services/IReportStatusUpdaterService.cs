using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces.Services
{
    public interface IReportStatusUpdaterService
    {
        void UpdateReportStatus(ReportStatus value);
    }
}