//using System;
//using System.Web.Mvc;
//using BrighterOption.Reports.Service.Web.App_Start;
//using BrighterOption.Reports.Service.Web.IoC;
//using BrighterOption.Reports.Web.Infrastructure.DependencyResolution;
//using WebActivator;
//
//[assembly: System.Web.PreApplicationStartMethod(typeof(DependencyInjectionConfig), "Start")]
//[assembly: ApplicationShutdownMethod(typeof(DependencyInjectionConfig), "Shutdown")]
//namespace BrighterOption.Reports.Service.Web.App_Start
//{
//    public class DependencyInjectionConfig
//    {
//        public static void Start()
//        {
//            try
//            {
//                //-- Initialize our DI container
//                var container = IoCForWebApi.Initialize();
//                var resolver = new StructureMapDependencyResolver(container);
//
//                //-- Configure ASP.NET MVC to use our container
//                DependencyResolver.SetResolver(resolver);
//
////                //-- Configure our DependencyHelper to use our container (used by classes that manually resolve their own dependencies)
////                DependencyHelper.SetResolver(resolver);
//            }
//            catch (Exception e)
//            {
//                // The site will be broken if we ever get here. But let's at least try to log what
//                // the problem is. 
//               // Exceptions.Log(e, LoggedExceptionSource.Web);
//                throw e;
//            }
//        }
//
//        public static void Shutdown()
//        {
//            IoCForWebApi.Shutdown();
//        }
//    }
//}