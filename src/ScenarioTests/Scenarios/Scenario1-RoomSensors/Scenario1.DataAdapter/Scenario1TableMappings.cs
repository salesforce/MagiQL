using System;
using System.Collections.Generic;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;

namespace Scenarios.Scenario1.DataAdapter
{
    public class Scenario1TableMappings : TableMappingsBase
    {
        public static TableMapping RoomTable = new TableMapping()
        {
            KnownTableName = KnownTables.Room,
            Alias = "r",
            DbTableName = "sc1.Room",
            PrimaryKey = "ID",
            TableType = TableType.Data
        };

        public static TableMapping RoomSensorTable = new TableMapping()
        {
            KnownTableName = KnownTables.RoomSensor,
            Alias = "rs",
            DbTableName = "sc1.RoomSensor",
            PrimaryKey = "RoomID",
            TableType = TableType.Data
        };
         

        public static List<TableRelationship> TableRelationships;

        public static readonly List<TableMapping> AllTables = new List<TableMapping>
        {
            RoomTable,
            RoomSensorTable
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
                OneToOne(RoomTable, RoomSensorTable, RoomSensorTable.PrimaryKey)
            };
        }

        public override string GetStatsTableName(string table, string joinTable, TemporalAggregation temporalAggregation,
            DateRangeType dateRangeType, DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }
    }
}