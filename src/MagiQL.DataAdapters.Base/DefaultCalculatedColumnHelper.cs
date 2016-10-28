using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumns;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;

namespace MagiQL.Reports.DataAdapters.Base
{
    public class DefaultCalculatedColumnHelper : CalculatedColumnHelperBase
    {
        protected readonly TableMappingsBase _tableMappings;
        protected readonly ConstantsBase _constants;
        protected readonly DefaultQueryHelpers QueryHelpers;
        
        protected readonly string _statsTableName;
        protected readonly string _transposeStatsTableName;

        public DefaultCalculatedColumnHelper(IColumnProvider columnProvider, TableMappingsBase tableMappings, ConstantsBase constants, DefaultQueryHelpers queryHelpers)
            : base(columnProvider, constants.DataSourceId)
        {
            _tableMappings = tableMappings;
            _constants = constants;
            QueryHelpers = queryHelpers;

            var statsTable = _tableMappings.GetAllTables().FirstOrDefault(x => x is StatsTableMapping);
            if (statsTable != null)
            {
                _statsTableName = statsTable.KnownTableName;
            }

            var transposeTable = tableMappings.GetAllTables().FirstOrDefault(x => x is TransposeStatsTableMapping);
            if (transposeTable != null)
            {
                _transposeStatsTableName = transposeTable.KnownTableName;
            }
        }
        
        protected override string GetStatsTable(bool useDateStats)
        {
            return _statsTableName;
        }
        
        public override string GetFieldAlias(ReportColumnMapping column)
        {
            return QueryHelpers.GetFieldAlias(column);
        }

        protected override ReportColumnMapping FindColumnByFieldName(string table, string field, FieldAggregationMethod aggregationMethod)
        {
            var foundTable = (_tableMappings.GetTableFromNameOrAlias(table) ?? table);

            int? transposeKeyValue = null;
            if (foundTable == _transposeStatsTableName)
            {
                transposeKeyValue = int.Parse(table.Split('_')[1]);
            }

            var matchingColumn = FindColumnByFieldName(foundTable, field, aggregationMethod, statTransposeKeyValue: transposeKeyValue);
            return matchingColumn;
        } 
 
        public virtual ReportColumnMapping FindColumnByFieldName(string table, string field, FieldAggregationMethod aggregationMethod, int? statTransposeKeyValue = null, bool cacheOnly = false)
        {
            var found = ColumnProvider.Find(_constants.DataSourceId, table, field, statTransposeKeyValue, cacheOnly);

            if (found.Count > 1)
            {
                found = found.Where(x => x.FieldAggregationMethod == aggregationMethod).ToList();
                if (found.Count > 1)
                {
                    if (statTransposeKeyValue > 0)
                    {
                        throw new Exception(string.Format("Found more than one column for {0}.{1} (transpose key value: {2})", table, field, statTransposeKeyValue));
                    }
                    throw new Exception(string.Format("Found more than one column for {0}.{1} (aggregation method : {2})", table, field, aggregationMethod));
                }
            }

            return found.SingleOrDefault();
        }
         
        protected override string GetTableAlias(KeyValuePair<ReportColumnMapping, string> foundColumn, string defaultTable)
        {
            var tableAlias = _tableMappings.GetTableAlias(foundColumn.Key.KnownTable);

            if (foundColumn.Key.KnownTable == _transposeStatsTableName)
            {
                tableAlias += "_" + foundColumn.Key.ActionSpecId;
            }
            return tableAlias;
        }
        
    }
}
