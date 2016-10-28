using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;

namespace Scenarios.Scenario2.DataAdapter
{
    public class Scenario2TableMappings : TableMappingsBase
    {
        public static TableMapping LocationHostTable = new TableMapping()
        {
            KnownTableName = KnownTables.LocationHost,
            Alias = "lh",
            DbTableName = "sc2.LocationHost",
            PrimaryKey = "ID",
            TableType = TableType.Data
        };

        public static TableMapping LocationTable = new TableMapping()
        {
            KnownTableName = KnownTables.Location,
            Alias = "l",
            DbTableName = "sc2.Location",
            PrimaryKey = "ID",
            TableType = TableType.Data
        };

        public static TableMapping LocationHitTable = new StatsTableMapping
        {
            KnownTableName = KnownTables.LocationHit, 
            DbTableName = "sc2.LocationHit",
            PrimaryKey = "ID"
        };
         

        public static List<TableRelationship> TableRelationships;

        public static readonly List<TableMapping> AllTables = new List<TableMapping>
        {
            LocationHostTable,
            LocationTable,
            LocationHitTable
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
                OneToMany(LocationHostTable, LocationTable, "LocationHostID"),
                OneToMany(LocationTable, LocationHitTable, "LocationID")
            };
        }

        public override string GetStatsTableName(string table, string joinTable, TemporalAggregation temporalAggregation,
            DateRangeType dateRangeType, DateTime? startDate, DateTime? endDate)
        {
            return LocationHitTable.DbTableName;
        }
    }
}