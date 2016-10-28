using System.Web.Http;

namespace MagiQL.Service.WebAPI.Routes.Controllers
{
    public class PingController : ApiController
    {
        //
        // GET: /Ping 
        public string Get()
        { 
            return "Pong";
        }

    }
}
