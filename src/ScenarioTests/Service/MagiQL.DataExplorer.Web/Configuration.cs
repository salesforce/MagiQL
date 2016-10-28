using System.Configuration;

namespace MagiQL.DataExplorer.Web
{
    public static class Configuration
    {
        public const int UserId = -1;

        public static class Exports
        {
            public static string FilePath
            {
                get { return ConfigurationManager.AppSettings["NETWORK_SHARE"]; }
            } 
            
            public static string FileNameFormat
            {
                get { return "report-{0}.xlsx";}
            }

        }
    }
}
