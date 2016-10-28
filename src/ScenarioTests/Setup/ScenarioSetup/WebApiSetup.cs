using System;
using System.Net.Http;
using Microsoft.Owin.Hosting;

namespace ScenarioSetup
{
    public class WebApiSetup : IDisposable
    {
        public IDisposable SelfHostedWebApp { get; set; }

        public void Start()
        {
            var baseUrl = "http://localhost:9443/";

            Console.WriteLine("Starting Website On {0}", baseUrl);

            SelfHostedWebApp = WebApp.Start<MagiQL.Service.Web.WebApiApplication>(baseUrl);

            using (var httpClient = new HttpClient())
            {
                var pingUrl = "http://localhost:9443/v1/ping";
                var requestUri = new Uri(pingUrl);
                var result = httpClient.GetStringAsync(requestUri).GetAwaiter().GetResult();
                Console.WriteLine("{0} returned {1}", pingUrl, result);
            }
        }

        public void Dispose()
        {
            SelfHostedWebApp.Dispose();
        }
    }
}
