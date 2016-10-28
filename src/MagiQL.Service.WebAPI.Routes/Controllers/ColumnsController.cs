using System.Web.Http;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class ColumnsController : ApiController
    {
        private readonly IReportsService _reportsService;
          
        public ColumnsController(IReportsService reportsService)
        {
            this._reportsService = reportsService;
        }

        // GET api/{platform}/Columns 
        public GetSelectableColumnsResponse Get(string platform, int? organizationId = null, int? userId = null, int? groupBy = null)
        {
            return _reportsService.GetSelectableColumns(platform, organizationId, userId, groupBy);
        } 

    }
} 