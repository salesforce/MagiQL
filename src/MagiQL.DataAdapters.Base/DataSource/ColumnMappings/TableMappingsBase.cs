using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings
{
    public abstract class TableMappingsBase
    {
        protected TableMappingsBase()
        {
            Initialize();
        }

        private void Initialize()
        {
            SetupRelationships();
        }

        public abstract List<TableMapping> GetAllTables();

        public abstract List<TableRelationship> GetAllTableRelationships();

        protected virtual bool IsStatsTable(string tableName)
        {
            return GetTableMapping(tableName).TableType == TableType.Stats;
        }
         
        protected abstract void SetupRelationships();

        protected TableRelationship ManyToOne(TableMapping table1, string table1Column, TableMapping table2, bool isDirect = true)
        {
            return new TableRelationship
            {
                Table1 = table1,
                Table1Column = table1Column,
                RelationshipType = TableRelationshipType.ManyToOne,
                Table2 = table2,
                Table2Column = table2.PrimaryKey,
                IsDirect = isDirect,
            };
        }

        protected TableRelationship OneToMany(TableMapping table1, TableMapping table2, string table2Column, bool isDirect = true)
        {
            return new TableRelationship
            {
                Table1 = table1,
                Table1Column = table1.PrimaryKey,
                RelationshipType = TableRelationshipType.OneToMany,
                Table2 = table2,
                Table2Column = table2Column,
                IsDirect = isDirect,
            };
        }

        protected TableRelationship OneToOne(TableMapping table1, string table1Column, TableMapping table2, bool isDirect = true)
        {
            return new TableRelationship
            {
                Table1 = table1,
                Table1Column = table1Column,
                RelationshipType = TableRelationshipType.OneToOne,
                Table2 = table2,
                Table2Column = table2.PrimaryKey,
                IsDirect = isDirect,
            };
        }
        protected TableRelationship OneToOne(TableMapping table1,  TableMapping table2, string table2Column,bool isDirect = true)
        {
            return new TableRelationship
            {
                Table1 = table1,
                Table1Column = table1.PrimaryKey,
                RelationshipType = TableRelationshipType.OneToOne,
                Table2 = table2,
                Table2Column = table2Column,
                IsDirect = isDirect,
            };
        }

      
        public virtual string GetTableAlias(string table)
        { 
            table = table.TrimStart('[').TrimEnd(']');
            if (table == "Data") return string.Empty;
            var result = GetAllTables().Where(x => x.KnownTableName == table || x.DbTableName == table).Select(x => x.Alias).FirstOrDefault();
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Alias not found for table " + table);
            }
            return result;
        }

        public virtual string GetTableFromNameOrAlias(string tableName)
        {
            tableName = tableName.Replace("[", "").Replace("]", "");
            string result = tableName;

            var fromAlias = GetAllTables().Where(x => x.Alias == tableName).Select(x => x.KnownTableName).FirstOrDefault();
            if (fromAlias != null)
            {
                result = fromAlias;
            }
            return result;
        }
         
        public virtual string GetTableName(string table, string joinTable = null, TemporalAggregation resolution = TemporalAggregation.Total)
        {
            if (IsStatsTable(table))
            {
                var tableMapping = GetAllTables().OfType<StatsTableMapping>().FirstOrDefault(x => x.KnownTableName == table);
                var dateTable = tableMapping.ResolutionTables.FirstOrDefault(x => x.JoinTable == joinTable && x.TemporalAggregation == resolution);
                if (dateTable == null)
                {
                    dateTable = tableMapping.ResolutionTables.FirstOrDefault(x => x.JoinTable == null && x.TemporalAggregation == resolution);
                }
                if (dateTable != null)
                {
                    return dateTable.DbTableName;
                }
            }
            else
            {
                var tableMapping = GetAllTables().FirstOrDefault(x => x.KnownTableName == table);
                if (tableMapping != null)
                {
                    return tableMapping.DbTableName;
                }
            }
            return table;
        }

        /// <summary>
        /// Returns the actual name of the database table that corresponds to
        /// the provided 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public virtual string GetDataTableName(string table)
        {
            if (IsStatsTable(table)) throw new ArgumentException(String.Format("Invalid value provided for 'table': [{0}]. This method can only be used to retrieve the name of non-stats tables. If you'd like to get the name of stats tables, use the GetStatsTableName() method instead.", table));

            var tableMapping = GetAllTables().FirstOrDefault(x => x.KnownTableName == table);
            if (tableMapping != null)
            {
                return tableMapping.DbTableName;
            }

            return table;
        }
        
        public virtual TableMapping GetTableMapping(string table)
        {
            var tableMapping = GetAllTables().FirstOrDefault(x => x.KnownTableName == table);

            return tableMapping;
        }

        /// <summary>
        /// Returns the name of the stats table or view we need to query
        /// to satisfy the provided temporal aggregation and date range.
        /// </summary>
        public abstract string GetStatsTableName(
            string table,
            string joinTable,
            TemporalAggregation temporalAggregation,
            DateRangeType dateRangeType,
            DateTime? startDate,
            DateTime? endDate);
    }
}