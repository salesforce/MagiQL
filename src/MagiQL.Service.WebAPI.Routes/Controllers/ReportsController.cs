using System.Web.Http;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class ReportsController : ApiController
    {
        private IReportsService reportsService;

        public ReportsController(IReportsService reportsService)
        {
            this.reportsService = reportsService;
        } 

        // GET api/{platform}/Reports/id
        public GetReportStatusResponse Get(string platform, long id, int? userId = null)
        {
            return reportsService.GetReportStatus(platform, id, userId);
        } 

        // POST api/{platform}/Reports
        public GenerateReportResponse Post(string platform, int? organizationId, [FromBody] SearchRequest request, int? userId = null)
        {
            return reportsService.GenerateReport(platform, organizationId, userId, request);
        }
    }
}