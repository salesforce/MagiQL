using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;
using SqlModeller.Compiler.QueryParameterManagers;
using SqlModeller.Compiler.SqlServer.SelectCompilers;
using SqlModeller.Model;
using SqlModeller.Shorthand;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Stats
{
    /// <summary>
    /// SELECT :
    ///  - _GroupKey
    ///  - _CurrencyKey
    ///  - Transpose Stats in requested columns
    ///  - Transpose Stats in order by
    ///  - Transpose Stats referenced in Calculated Columns
    /// 
    /// COLUMN NAMING : 
    ///  - FieldAlias matches original field name
    /// 
    /// FROM : 
    ///  - Data table joining to the stats table
    /// 
    /// GROUP BY : 
    ///  - Currency Code (_CurrencyKey)
    ///  - Primary key of the data table joining to the stats table
    /// 
    /// WHERE : 
    ///  - Matching TrasposeKey
    ///  - If DateQuery : Date Range
    /// 
    /// </summary>
    public class DefaultTransposeStatsCteQueryBuilder : QueryBuilderBase
    {
        protected readonly string _transposeStatsTableName;
        protected readonly string _transposeStatsTableAlias;
        protected readonly string _transposeStatsKeyFieldAlias;
        protected readonly int _transposeKeyColumnId;
        protected readonly IDataSourceComponents _dataSourceComponents;
        
        public DefaultTransposeStatsCteQueryBuilder(IDataSourceComponents dataSourceComponents) : base(dataSourceComponents)
        {
            _dataSourceComponents = dataSourceComponents;

            var transposeStatsTable = dataSourceComponents.TableMappings.GetAllTables().FirstOrDefault(x => x is TransposeStatsTableMapping);
            if (transposeStatsTable != null)
            {

                _transposeStatsTableName = transposeStatsTable.KnownTableName;
                _transposeStatsTableAlias = transposeStatsTable.Alias;

                var transposeKeyField = ((TransposeStatsTableMapping) transposeStatsTable).TransposeKey;
                _transposeStatsKeyFieldAlias = "_" + transposeKeyField;

                 var transposeKeyColumn = _columnProvider.Find(_constants.DataSourceId, _transposeStatsTableName, transposeKeyField, null);
                _transposeKeyColumnId = transposeKeyColumn.First().Id; 
            }

        }

        // Build
        public override void BuildSelect(SelectQuery query, List<ReportColumnMapping> selectedColumns, ReportColumnMapping sortColumn, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            // SELECT
            query.SelectGroupKey(_constants.GroupKeyAlias);
            query.SelectGroupKey(_transposeStatsKeyFieldAlias, 1);

            if (request.TemporalAggregation != TemporalAggregation.Total)
            {
                query.SelectGroupKey(_constants.DateKeyAlias, 2);
            }

            var allActionStatsColumns = _dataSourceComponents.StatsQueryBuilder.GetAllTransposeStatsColumns(request);

            var selectedFields = new List<string>(); // used to stop selecting the same column twice
            foreach (var column in selectedColumns)
            {
                // dont add the same column twice - quite possible because of the same column being used with multiple transpose ids
                if (!selectedFields.Contains(column.FieldName))
                {

                    // theres a complex scenario here when we need to select mixed aggregation modes.
                    if (allActionStatsColumns.Any(x => x.FieldName == column.FieldName && x.FieldAggregationMethod != column.FieldAggregationMethod))
                    {
                        // in this case we need to select the same column with different aggregation methods dependant on the transpose id
                        SelectWithAggregationSwitch(query, allActionStatsColumns, column, request);
                    }
                    else
                    {
                        // SELECT simple
                        var aggregate = MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.GetAggregate(column.FieldAggregationMethod);
                        var colName = GetColumnSelector(column, request);
                        query.Select(colName.TableAlias, colName.Field.Name, column.FieldName, aggregate);
                    } 
                }
            }
        }
         

        private void SelectWithAggregationSwitch(SelectQuery query, List<ReportColumnMapping> allActionStatsColumns, ReportColumnMapping column, MappedSearchRequest request)
        {
            var columnsByAggregationMode = allActionStatsColumns.Where(x => x.FieldName == column.FieldName).GroupBy(x => x.FieldAggregationMethod);

            var colName = GetColumnSelector(column, request);

            var caseSql = "CASE";

            foreach (var agg in columnsByAggregationMode)
            {
                var aggregate = MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.GetAggregate(agg.Key);
                var transposeIds = agg.Select(x => x.ActionSpecId).Distinct();

                // use the column compiler to generate the aggregation - but need to remove the column alias after
                var selectCompiler = new ColumnSelectorCompiler();
                var q = new SelectQuery().Select(colName.TableAlias, colName.Field.Name, "REMOVEME", aggregate);
                var selectSql = selectCompiler.Compile(q.SelectColumns.First(), q, new NoQueryParameterManager()).Replace(" AS REMOVEME", "");
                caseSql += "\n\t   WHEN aStats.ActionSpecID IN (" + string.Join(",", transposeIds) + ")\n\t    THEN " + selectSql;
            }

            caseSql += "\n\t END AS " + column.FieldName;

            query.Select(caseSql);
        }



        public override void BuildFrom(SelectQuery query, List<ReportColumnMapping> selectedColumns, ReportColumnMapping groupByColumn, ReportColumnMapping sortByColumn, MappedSearchRequest request)
        {
            var statsRelationships = _tableMappings.GetAllTableRelationships()
              .Where(x => x.Table1 is StatsTableMapping || x.Table2 is StatsTableMapping)
              .ToList();
              
            // get the data table which joins onto the stats table 
            var graphBuilder = new TableRelationshipGraphBuilder();
            var dataJoinTable = _dataSourceComponents.StatsQueryBuilder.GetDataJoinTable(request.GroupByColumn, statsRelationships, graphBuilder);

            //-- Find out which stats table or view we need to query to satisfy the requested temporal
            // aggregation and type of date range.
            var statsTableName = GetStatsTableName(
                _transposeStatsTableName,
                dataJoinTable, 
                request.TemporalAggregation,
                request.DateRangeType,
                request.DateStart, 
                request.DateEnd);

            query.From(statsTableName, _transposeStatsTableAlias);
        }

        protected override void BuildWhere(SelectQuery query, string queryText, List<MappedSearchRequestFilter> filters, MappedSearchRequest request)
        {

            var allTransposeStatsColumns = _dataSourceComponents.StatsQueryBuilder.GetAllTransposeStatsColumns(request);
            var transposeKeyColumn = QueryHelpers.GetColumnMapping(_transposeKeyColumnId);
            var selectedTransposeKeyValues = allTransposeStatsColumns.Select(x => x.ActionSpecId).Distinct().Select(x => x.ToString()).ToList();
            query.WhereColumnIn(_transposeStatsTableAlias, transposeKeyColumn.FieldName, selectedTransposeKeyValues, DbType.Int32, _transposeStatsKeyFieldAlias);

            StatsQueryHelpers.AddDateFilters(query, _transposeStatsTableAlias, request.TemporalAggregation, request.DateRangeType, _constants.StatsDateDbField, request.DateStart, request.DateEnd);
             
        }

        protected override void BuildGroupBy(SelectQuery query, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            var actionSpecIdColumn = QueryHelpers.GetColumnMapping(_transposeKeyColumnId);

            query.GroupBy(_transposeStatsTableAlias, groupByColumn.FieldName)
                 .GroupBy(_transposeStatsTableAlias, actionSpecIdColumn.FieldName);

            StatsQueryHelpers.AddTemporalAggregationGroupBy(query, request.TemporalAggregation, _constants.StatsDateDbField);
        }

        protected override void BuildHaving(SelectQuery query, List<MappedSearchRequestFilter> list, ReportColumnMapping reportColumnMapping, MappedSearchRequest request)
        {
            // not allowed
        }

        protected override void BuildOrderBy(SelectQuery query, ReportColumnMapping reportColumnMapping, MappedSearchRequest request)
        {
            // not allowed
        }
         
        // Restrict
        protected override List<ReportColumnMapping> RestrictColumns(MappedSearchRequest request)
        {

            var allTransposeStatColumns = _dataSourceComponents.StatsQueryBuilder.GetAllTransposeStatsColumns(request)
                // calculations are not performed in the transpose stats query
                .Where(x => !x.IsCalculated);

            var result = new List<ReportColumnMapping>();
            
            // add any columns needed by calculations 
            foreach (var found in allTransposeStatColumns)
            {
                if (result.All(x => x.FieldName != found.FieldName))
                {
                    result.Add(found);
                }
            }
             
            return result;
        }

        protected override ReportColumnMapping RestrictGroupBy(MappedSearchRequest request)
        {
            var statsRelationships = _tableMappings.GetAllTableRelationships()
                      .Where(x => x.Table1 is StatsTableMapping || x.Table2 is StatsTableMapping)
                      .ToList();
             

            // get the data table which joins onto the stats table 
            var graphBuilder = new TableRelationshipGraphBuilder();
            var dataJoinTable = _dataSourceComponents.StatsQueryBuilder.GetDataJoinTable(request.GroupByColumn, statsRelationships, graphBuilder);
             
            // get the stats relationship to the data join table
            var statsRelationship = statsRelationships.FirstOrDefault(x => x.Table1.KnownTableName == dataJoinTable || x.Table2.KnownTableName == dataJoinTable);

            var statForeignKeyToDataTable = statsRelationship.Table1.KnownTableName == dataJoinTable
                       ? statsRelationship.Table2Column
                       : statsRelationship.Table1Column;

            var groupByColumn = _columnProvider.Find(_constants.DataSourceId, _transposeStatsTableName, statForeignKeyToDataTable, null).First();
            return groupByColumn; 
              
        }
        
        // Override
        public override Column GetColumnSelector(ReportColumnMapping col, MappedSearchRequest request, bool dontAggregate = false, bool useFieldAlias = false)
        {  
            return new Column(_transposeStatsTableAlias, col.FieldName);
        }
        
        public override ReportColumnMapping GetCurrencyColumn()
        {
            return _columnProvider.GetColumnMapping(_constants.DataSourceId, _constants.CurrencyColumnId);
        }
        
        public override bool GroupByIsChildTable(
            string tableAlias,
            ReportColumnMapping groupByColumn)
        {
            throw new NotImplementedException();
        }
        
        public override bool IsDateColumn(ReportColumnMapping x)
        {
            throw new NotImplementedException();
        }

        public override bool RequireCurrencyGroupBy(MappedSearchRequest request)
        {
            throw new NotImplementedException();
        }
        
        public override bool WhereFilterColumnAllowIsNull(MappedSearchRequestFilter filter)
        {
            return _dataSourceComponents.QueryBuilderBase.WhereFilterColumnAllowIsNull(filter);
        }
    }
}
