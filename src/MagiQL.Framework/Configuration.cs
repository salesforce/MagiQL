using System.Configuration;

namespace MagiQL.Framework
{
    public static class Configuration
    {
        public static class Environment
        {
            public static string MachineName = System.Environment.MachineName;
        }

        public static class Exports
        {
            /// <summary>
            /// How many rows should be loaded in a single request
            /// </summary>
            public const int DefaultPageSize = 100;

            /// <summary>
            /// How much of the percentage progress is related to loading the report
            /// </summary>
            public const double DataLoadPercent = 0.7;

            /// <summary>
            /// How much of the percentage progress is related to saving the report
            /// </summary>
            public const double DataSavePercent = 1 - DataLoadPercent;

            /// <summary>
            /// How frequently the long running report generator should write the updated status
            /// </summary>
            public const int UpdateStatusFrequencySeconds = 5;

            /// <summary>
            /// How long the status save timer can run before it shuts off automatically
            /// </summary>
            public const int MaxTimerPeriod = 60 * 30; // 30 minutes

            /// <summary>
            /// The folder path to save exported files
            /// </summary>
            public static string FilePath
            {
                get { return ConfigurationManager.AppSettings["NETWORK_SHARE"]; }
            }

            /// <summary>
            /// The file name format to use when saving spreadsheets
            /// </summary>
            public static string FileNameFormat
            {
                get { return "report-{0}.xlsx"; }
            }
        }
    }
}
