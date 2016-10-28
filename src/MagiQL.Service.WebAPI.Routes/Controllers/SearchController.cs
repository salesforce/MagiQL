using System.Web.Http;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;
using System;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    [Obsolete("Use /query instead")]
    public class SearchController : ApiController
    {
        private readonly IReportsService reportsService;
          
        public SearchController(IReportsService reportsService)
        {
            this.reportsService = reportsService;
        }

        // POST api/{platform}/search
        public SearchResponse Post(string platform, int? organizationId, [FromBody] SearchRequest request, int? userId = null)
        {
            return reportsService.Search(platform, organizationId, userId, request);
        }
    }
}