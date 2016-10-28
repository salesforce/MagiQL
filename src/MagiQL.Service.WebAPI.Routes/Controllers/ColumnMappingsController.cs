using System.Web.Http;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class ColumnMappingsController : ApiController
    {
        private readonly IReportsService _reportsService;

        public ColumnMappingsController(IReportsService reportsService)
        {
            this._reportsService = reportsService;
        }

        // GET api/{platform}/ColumnMappings
        public GetColumnMappingsResponse Get(string platform, int organizationId, int? userId = null, int? columnId = null, bool clearCache = false)
        {
            return _reportsService.GetColumnMappings(platform, organizationId, userId, columnId, clearCache);

        }
         
        // POST api/{platform}/ColumnMappingsmapping/
        public CreateColumnMappingResponse Post(string platform, [FromBody] ReportColumnMapping mapping)
        {
            return _reportsService.CreateColumnMapping(platform, mapping);
        }

        // PUT api/{platform}/ColumnMappings/{id}
        public UpdateColumnMappingResponse Put(string platform, int id, [FromBody] ReportColumnMapping mapping)
        {
            return _reportsService.UpdateColumnMapping(platform, columnId: id, userId:null, columnMapping: mapping );
        } 

    }
}