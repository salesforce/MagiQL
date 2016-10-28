using MagiQL.Reports.DataAdapters.Base;

namespace Scenarios.Scenario3.DataAdapter
{
    public class Constants : ConstantsBase
    {
        public override int DataSourceId { get { return 3; } }

        public override string Platform{get { return "Scenario3"; }}

        public override string RootTableName { get { return KnownTables.Team; } }

        public override string TextSearchColumnUniqueName { get { return "Player_Name"; } }

        public override string StatsDateDbField { get { return "TimeStampUTC"; } }

        public override string ConnectionStringName { get { return "MagiQL"; } }
    }

    
}