using MagiQL.Service.WebAPI.StructureMap.IoC.MagiQL;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace MagiQL.Service.WebAPI.StructureMap.IoC
{
    /// <summary>
    /// Small helper class for DI container container initialization in 
    /// web apps.
    /// </summary>
    public static class IocForWebApi
    {
        /// <summary>
        /// Creates and initializes a StructureMap container 
        /// If your application requires additional
        /// registries that are specific to that application, you can provide them 
        /// via the 'additionalRegistries' parameter.
        /// </summary>
        public static IContainer InitializeContainer(params Registry[] additionalRegistries)
        {
            var container = new Container(x =>
            {
                x.IncludeRegistry<InfrastructureWeb>();
                x.IncludeRegistry<MagiQlRegistry>();

                if (additionalRegistries != null)
                {
                    foreach (var additionalRegistry in additionalRegistries)
                    {
                        x.IncludeRegistry(additionalRegistry);
                    }
                }
            });
             
            return container;
        }
    }

    public class InfrastructureWeb : Registry
    {
        public InfrastructureWeb()
        {
            // Enable property injection on HTTP handlers as we can't do constructor injection with them 
            // (StructureMap doesn't do property injection by default)
            Policies.SetAllProperties(s => s.Matching(p => p.DeclaringType != null && p.DeclaringType.GetInterface("IHttpHandler") != null));
        }
    }
}