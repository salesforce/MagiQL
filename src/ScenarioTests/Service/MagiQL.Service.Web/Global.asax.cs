using System;
using System.Reflection;
using System.Web.Http; 
using log4net;
using MagiQL.Framework.Interfaces;
using MagiQL.Service.WebAPI.Routes;
using MagiQL.Service.WebAPI.StructureMap;
using Owin;
using Scenarios.Scenario1.DataAdapter;
using Scenarios.Scenario2.DataAdapter;
using Scenarios.Scenario3.DataAdapter;

namespace MagiQL.Service.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        // enable if you want log4net
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            try
            {
                Configure(GlobalConfiguration.Configuration);
            }
            catch (Exception e)
            {
                try
                {
                    // enable if you want log4net
                    Log.Error(e);
                    System.Diagnostics.Debug.WriteLine(e);
                }
                catch
                {
                    // We're stuffed. 
                }

                throw;
            }
        }

        internal static void Configure(HttpConfiguration configuration)
        {
            // enable if you want log4net
            //log4net.Config.XmlConfigurator.Configure();  

            // setup structuremap and configure datasources
            configuration.ConfigureStructureMapForMagiQL<WebApiApplication, NullLoggingProvider>(
                x =>
                {
                    x.For<IReportsDataSource>().Use<Scenario1DataSource>();
                    x.For<IReportsDataSource>().Use<Scenario2DataSource>();
                    x.For<IReportsDataSource>().Use<Scenario3DataSource>();
                });

            // use the MagiQL API Controllers
            configuration.UseMagiQLApi();

            //CustomisationsRegistration.RegisterAllCustomisations();
        }

        /// <summary>
        ///  required for OWIN self hosting
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();
            //WebApiConfig.Register(configuration);
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters); 
            
            WebApiApplication.Configure(configuration);

            app.UseWebApi(configuration);
        }
    }

   
}