using System;
using System.Web.Http;
using MagiQL.Framework.Interfaces.Logging;
using MagiQL.Service.WebAPI.StructureMap.IoC;
using StructureMap;


namespace MagiQL.Service.WebAPI.StructureMap
{
    public static class ApplicationStart
    {  

        // Keep track of our container so that we can dispose it on shutdown
        private static IContainer structureMapContainer;
         

        /// <summary>
        /// Execute the pre-start initialization steps for this application.
        /// 
        /// This will configure the config system, the DI container and other infrastructure-level stuff.
        /// 
        /// Runs this once on application startup before doing anything else.
        /// </summary>
        public static void ConfigureStructureMapForMagiQL<TWeb, TLog>(this HttpConfiguration httpConfiguration, Action<ConfigurationExpression> registration) where TLog : ILoggingProvider
        {
            try
            {
                //-- Initialize StructureMap
                structureMapContainer = IocForWebApi.InitializeContainer();
                structureMapContainer.Configure(x =>
                        {
                            x.For<ILoggingProvider>().Use<TLog>();
                            x.Scan(Registration.UseDefaultConventions<TWeb>);

                            // all datasource implementations
                            registration(x);
                        }
                    );

                //-- Register WebAPI to resolve their dependencies using our StructureMap container
                httpConfiguration.DependencyResolver = new WebApiDependencyResolver(structureMapContainer, false);

                //-- Resolve our global application-level services
                GlobalServices.Initialize(structureMapContainer); 
                
            }
            catch (Exception e)
            {
                // OK, well, we're stuffed.
                System.Diagnostics.Debug.WriteLine(e);
               
                throw;
            }
        }
          

        public static void Shutdown()
        {
            if (structureMapContainer != null)
            {
                structureMapContainer.Dispose();
                structureMapContainer = null;
            }
        }
    }

  
}