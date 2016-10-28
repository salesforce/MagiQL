using System.Web.Mvc;
using System.Web.Routing;

namespace MagiQL.DataExplorer.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute(
                name: "FacebookHome",
                url: "facebook",
                defaults: new { controller = "Home", action = "Index", platform = "facebook" }
            );

            routes.MapRoute(
              name: "TwitterHome",
              url: "twitter",
              defaults: new { controller = "Home", action = "Index", platform = "twitter" }
            );

            routes.MapRoute(
                name: "ExportDownload",
                url: "{platform}/ExportStatus/Download/{id}",
                defaults: new { controller = "ExportStatus", action = "Download", platform = UrlParameter.Optional, id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ExportStatus",
                url: "{platform}/ExportStatus/{id}",
                defaults: new { controller = "ExportStatus", action = "Index", platform = UrlParameter.Optional, id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ColumnManager",
                url: "{platform}/ColumnManager",
                defaults: new { controller = "ColumnManager", action = "Index", platform = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "DefaultPlatform",
               url: "{platform}/{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, platform = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}