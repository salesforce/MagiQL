using System.Web.Http;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Interfaces;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class PlatformsController : ApiController
    {
        private IReportsService reportsService;

        public PlatformsController(IReportsService reportsService)
        {
            this.reportsService = reportsService;
        } 

        public GetPlatformsResponse Get()
        {
            return reportsService.GetPlatforms();
        } 
         
    }
}