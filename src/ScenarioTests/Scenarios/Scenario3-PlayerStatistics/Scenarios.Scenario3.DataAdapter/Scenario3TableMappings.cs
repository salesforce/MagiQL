using System;
using System.Collections.Generic;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;

namespace Scenarios.Scenario3.DataAdapter
{
    public class Scenario3TableMappings : TableMappingsBase
    {
          
        public static TableMapping TeamTable = new TableMapping()
        {
            KnownTableName = KnownTables.Team,
            Alias = "t",
            DbTableName = "sc3.Team",
            PrimaryKey = "ID",
            TableType = TableType.Data
        };

        public static TableMapping MatchTable = new TableMapping()
        {
            KnownTableName = KnownTables.Match,
            Alias = "m",
            DbTableName = "sc3.Match",
            PrimaryKey = "ID",
            TableType = TableType.Data
        };

        public static TableMapping PlayerTable = new TableMapping
        {
            KnownTableName = KnownTables.Player,
            Alias = "p",
            DbTableName = "sc3.Player",
            PrimaryKey = "ID"
        };


        public static TableMapping PlayerPhysicalAttributesTable = new TableMapping
        {
            KnownTableName = KnownTables.PlayerPhysicalAttributes,
            Alias = "ppa",
            DbTableName = "sc3.PlayerPhysicalAttributes",
            PrimaryKey = "PlayerID"
        };

        public static TableMapping PlayerAchievementsTable = new TableMapping
        {
            KnownTableName = KnownTables.PlayerAchievements,
            Alias = "pa",
            DbTableName = "sc3.PlayerAchievements",
            PrimaryKey = "ID"
        };

        public static TableMapping PlayerMatchStatisticsTable = new TableMapping
        {
            KnownTableName = KnownTables.PlayerMatchStatistics,
            Alias = "pms",
            DbTableName = "sc3.PlayerMatchStatistics",
            PrimaryKey = "ID"
        };
         

        public static List<TableRelationship> TableRelationships;

        public static readonly List<TableMapping> AllTables = new List<TableMapping>
        {
            TeamTable,
            MatchTable,
            PlayerTable,
            PlayerPhysicalAttributesTable,
            PlayerAchievementsTable,
            PlayerMatchStatisticsTable
        };


        public override List<TableMapping> GetAllTables()
        { 
            return AllTables;
        }

        public override List<TableRelationship> GetAllTableRelationships()
        {
            return TableRelationships;
        }

        protected override void SetupRelationships()
        {
            if (TableRelationships != null)
            {
                return;
            }

            TableRelationships = new List<TableRelationship>
            {
                OneToMany(TeamTable, PlayerTable, "TeamID"),
                OneToOne(PlayerTable, PlayerPhysicalAttributesTable, "PlayerID"),
                OneToMany(PlayerTable, PlayerAchievementsTable, "PlayerID"),
                OneToMany(PlayerTable, PlayerMatchStatisticsTable, "PlayerID", isDirect: true),
                OneToMany(TeamTable, MatchTable, "HomeTeamId", isDirect: false),
                OneToMany(MatchTable, PlayerMatchStatisticsTable, "MatchId", isDirect: true),
                
            };
        }

        public override string GetStatsTableName(string table, string joinTable, TemporalAggregation temporalAggregation,
            DateRangeType dateRangeType, DateTime? startDate, DateTime? endDate)
        {
            return PlayerMatchStatisticsTable.DbTableName;
        }
    }
}