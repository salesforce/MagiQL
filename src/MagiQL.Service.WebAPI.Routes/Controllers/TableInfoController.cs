using System.Web.Http;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class TableInfoController : ApiController
    {
        private readonly IReportsService _reportsService;

        public TableInfoController(IReportsService reportsService)
        {
            this._reportsService = reportsService;
        }

        // GET api/{platform}/Columns 
        public GetTableInfoResponse Get(string platform)
        {
            return _reportsService.GetTableInfo(platform);
        }

    }
}