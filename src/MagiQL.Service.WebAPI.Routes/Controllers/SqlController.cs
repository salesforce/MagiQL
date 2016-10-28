using System.Web.Http;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class SqlController : ApiController
    {
        private readonly IReportsService reportsService;
          
        public SqlController(IReportsService reportsService)
        {
            this.reportsService = reportsService;
        }

        // POST api/{platform}/search
        public SearchResponse Post(string platform, int? organizationId, [FromBody] SearchRequest request, int? userId = null)
        {
            request.DebugMode = true;
            return reportsService.Sql(platform, organizationId, userId, request);
        }
    }
}