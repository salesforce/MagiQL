using System.Net.Http;
using System.Web.Http;
using MagiQL.Service.WebAPI.Routes.ActionFilters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MagiQL.Service.WebAPI.Routes
{
    public static class Registration
    {

        public static void UseMagiQLApi(this HttpConfiguration config)
        {
            Register(config);
        }

        public static void Register(HttpConfiguration config)
        {
            RegisterWebApiRoutes(config);


            RegisterGlobalFilters(config);

            ConfigureJsonSerialization(config);

            // We don't want to support XML. While it initially comes for free, it will
            // cost us in the future to maintain if for some bizarre reason someone decides
            // to actually use it.
            RemoveXmlFormatter(config);
        }


        public static void RegisterWebApiRoutes(HttpConfiguration config, string versionSegment = "v1")
        {
            // The Reporting API relies entirely on convention-based routing.
            // No attribute routing or custom routing. 
            config.Routes.MapHttpRoute(
                name: string.Format("MagiQL_{0}_DefaultApi", versionSegment),
                routeTemplate: versionSegment + "/{platform}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            config.Routes.MapHttpRoute(
                name: string.Format("MagiQL_{0}_Ping", versionSegment),
                routeTemplate: versionSegment + "/ping",
                defaults: new { controller = "Ping", id = RouteParameter.Optional }
                ); 

            config.Routes.MapHttpRoute(
                name: string.Format("MagiQL_{0}_Platforms", versionSegment),
                routeTemplate: versionSegment + "/platforms",
                defaults: new { controller = "Platforms", id = RouteParameter.Optional }
                );
        }

        public static void RegisterGlobalFilters(HttpConfiguration config)
        {
            // This filter will set the response Status Code to '500 Server Error'
            // whenever an action returns an error response.
            config.Filters.Add(new StatusCodeResponseActionFilter());
             
        }
        
        public static void ConfigureJsonSerialization(HttpConfiguration config)
        {
            // Pretty please
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;

            // camelCase for our properties
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // That's the default anyway but make it explicit to avoid any possible confusion
            config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

            // The doc for Json.NET's DateTimeZoneHandling is here http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_DateTimeZoneHandling.htm
            // and is as vague and unclear as it could be. 
            //
            // They have some sample code that does demonstrate the behaviour though: http://www.newtonsoft.com/json/help/html/SerializeDateTimeZoneHandling.htm
            //
            // DateTimeZoneHandling.RoundtripKind has the following behaviour:
            // 
            // On deserialization:
            // 1) If the incoming date is a local date (i.e. a date without a UTC offset), e.g. "2015-06-15T00:00", JSON.NET will
            // deserialize it as-it (no timezone conversion) into a DateTime object with a Kind of DateTimeKind.Unspecified.
            // 
            // 2) If the incoming date is a specific instant in time (i.e. date with a UTC offset), e.g. "2015-06-15T00:00Z" or
            // "2015-06-15T00:00+02:00", JSON.NET will first convert it to UTC using the provided UTC offset and then store it
            // in a DateTime object with its Kind set to DateTimeKind.Utc.
            //
            // => this is exactly what we want. When users of our API provide us with an Absolute date range, we want to get the UTC
            // values of the dates they provided. Because dates and times are always manipulated in UTC in our backend.  
            //
            // But when they provide us with a date range expressed in Account Timezone, we don't
            // want any timezone conversion to happen as the provided dates are *not* instants in time. They're just textual representation
            // of a date and a time which will correspond to a different instant in time depending on the ad account in question. 
            // So we want these to get stored as-it in a DateTime with an "Unspecified" kind (i.e. a DateTime that represents neither
            // UTC time nor Local Machine time). 
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
        }

        public static void RemoveXmlFormatter(HttpConfiguration config)
        {
            var xmlFormatter = config.Formatters.XmlFormatter;

            if (xmlFormatter != null)
                config.Formatters.Remove(xmlFormatter);
        }

    }
}
