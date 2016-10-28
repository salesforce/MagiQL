using System.Web.Http;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class ConfigurationController : ApiController
    {
        private readonly IReportsService _reportsService;

        public ConfigurationController(IReportsService reportsService)
        {
            this._reportsService = reportsService;
        }

        // GET api/{platform}/configuration
        public GetConfigurationResponse Get(string platform)
        {
            return _reportsService.GetConfiguration(platform);

        }
    }
}
