using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces.Services
{
    public interface IAsyncReportGeneratorService
    {
        ReportStatus Setup(string platform, int? organizationId, int? userId, SearchRequest request, string filePath);

        ReportStatus GetStatus(long requestId);
    }
}