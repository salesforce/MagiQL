 using MagiQL.Reports.DataAdapters.Base;

namespace Scenarios.Scenario2.DataAdapter
{
    public class Constants : ConstantsBase
    {
        public override int DataSourceId { get { return 2; } }

        public override string Platform{get { return "Scenario2"; }}

        public override string RootTableName { get { return KnownTables.LocationHost; } }

        public override string TextSearchColumnUniqueName { get { return "LocationHost_Host"; } }

        public override string DateStatColumnUniqueName { get { return "LocationHit_TimeStampUTC"; }}
          
        public override string StatsDateDbField{get { return "TimeStampUTC"; }}

        public override string ConnectionStringName { get { return "MagiQL"; } }
    } 
}