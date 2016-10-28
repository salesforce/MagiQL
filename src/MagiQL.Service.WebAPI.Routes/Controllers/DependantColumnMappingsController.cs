using System;
using System.Web.Http;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class DependantColumnMappingsController : ApiController
    {
        private readonly IReportsService _reportsService;

        public DependantColumnMappingsController(IReportsService reportsService)
        {
            this._reportsService = reportsService;
        }

        // GET api/{platform}/DependantColumnMappings
        public GetColumnMappingsResponse Get(string platform, int? columnId = null, string fieldName = null)
        {
            if (columnId != null && fieldName != null)
            {
                throw new Exception("Only one of columnId and fieldName are allowed");
            }
            if (columnId != null)
            {
                return _reportsService.GetDependantColumns(platform, columnId.Value);
            }
            if (fieldName != null)
            {
                return _reportsService.GetDependantColumns(platform, fieldName);
            }
            throw new Exception("One of columnId or fieldName must be supplied");
        }
         

    }
}