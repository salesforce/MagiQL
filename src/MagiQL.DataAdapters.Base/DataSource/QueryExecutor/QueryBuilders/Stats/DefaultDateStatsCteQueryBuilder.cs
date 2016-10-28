using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using SqlModeller.Model;
using SqlModeller.Shorthand;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Stats
{
    // describing the complexity of selecting date ranges : https://docs.google.com/a/salesforce.com/drawings/d/1ao16VlOgMfnIZU3UV8DHsZIUeizWeRwZ_VRb6spJSjU/edit

    // Date Stats are calculated seperately to lifetime stats so that we can support formulas which reference both

    /// <summary>
    /// DateStatsCteQueryBuilder builds a CTE that selects from either the Daily or the Hourly stats table. 
    /// 
    /// This is used when either:
    /// - We've been asked to query stats over a specific date range (so using the Lifetime table wouldn't work), or
    /// - We've been asked to return the results of the query broken down by day or by hour (i.e. TemporalAggregation = 
    /// ByDay or ByHour)
    /// 
    /// SELECT :
    ///  - _GroupKey
    ///  - _C = Count 
    ///  - If Order By is a Stat (not action stat or calculated) : select _R (row number, for sorting later)
    ///  - Date stats in requested columns
    ///  - Exclude transpose stats columns 
    ///  - Date stats which are used in calculations - calculations are performed in the stats query  
    ///  - Date stats referenced in Calculated Column used for sorting
    ///  - Date stats used for sorting 
    ///  
    /// COLUMN NAMING : 
    ///  -  Non-calculated columns use their own name as the alias
    ///  -  Calculated columns use a generated alias
    /// 
    /// FROM : 
    ///  - Pick the right stats table based on group by (find closest table in relationships) 
    /// 
    /// GROUP BY :  
    /// 
    /// WHERE : 
    ///  - If DateQuery : Date Range
    /// 
    /// </summary>
    public class DefaultDateStatsCteQueryBuilder : DefaultStatsQueryBuilder 
    {
        protected List<int> _foreignKeyColumnIds = new List<int>();

        public DefaultDateStatsCteQueryBuilder(IDataSourceComponents dataSourceComponents) : base(dataSourceComponents)
        {
            var statsTable = dataSourceComponents.TableMappings.GetAllTables().FirstOrDefault(x => x is StatsTableMapping);
            if (statsTable != null)
            {
                _statsTableName = statsTable.KnownTableName;
                _statsTableAlias = statsTable.Alias;
            }
        }
        
        // Build 
        public override void BuildFrom(
            SelectQuery query,
            List<ReportColumnMapping> selectedColumns,
            ReportColumnMapping groupByColumn,
            ReportColumnMapping sortByColumn,
            MappedSearchRequest request)
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
                _statsTableName,
                dataJoinTable,
                request.TemporalAggregation,
                request.DateRangeType,
                request.DateStart,
                request.DateEnd);

            query.From(statsTableName, _statsTableAlias);
            JoinTables(query, dataJoinTable, _statsTableName);

            // join up to the root table
            var addedTables = new Dictionary<string, bool> { { dataJoinTable, false } };
            _dataSourceComponents.StatsQueryBuilder.AddDataTableJoinsUpToRoot(query, dataJoinTable, graphBuilder, addedTables);

            var currencyColumn = GetCurrencyColumn();
            if (currencyColumn != null && !addedTables.ContainsKey(currencyColumn.KnownTable))
            {
                var relationshipGraph = graphBuilder.Build(_tableMappings.GetAllTableRelationships(), _constants.RootTableName);
                AddFromTableJoin(query, groupByColumn, currencyColumn.KnownTable, ref addedTables, graphBuilder, relationshipGraph);
            }
        }
        
        protected override void BuildWhere(SelectQuery query,string queryText,List<MappedSearchRequestFilter> filters,MappedSearchRequest request)
        {
            StatsQueryHelpers.AddDateFilters(query, _statsTableAlias, request.TemporalAggregation, request.DateRangeType, _constants.StatsDateDbField, request.DateStart, request.DateEnd);
        }

        protected override void BuildHaving(SelectQuery query, List<MappedSearchRequestFilter> list, ReportColumnMapping reportColumnMapping, MappedSearchRequest request)
        {
            // not allowed
        }

        protected override void BuildOrderBy(SelectQuery query, ReportColumnMapping reportColumnMapping, MappedSearchRequest request)
        {
            // not allowed
        }

        public override Column GetColumnSelector(ReportColumnMapping col, MappedSearchRequest request, bool dontAggregate = false, bool useFieldAlias = false)
        {
            var result = _dataSourceComponents.QueryBuilderBase.GetColumnSelector(col, request, dontAggregate: dontAggregate, useFieldAlias: useFieldAlias);

            if (result.Field.Name.StartsWith("_C_"))
            {
                result.Field.Name = _constants.CountKeyAlias;
            }

            return result;
        }

        protected override bool CanSelectRowNumber()
        {
            return false;
        }

        // CTE
        protected override List<CommonTableExpression> BuildCommonTableExpressions(MappedSearchRequest request)
        {
            return new List<CommonTableExpression>();
        }
        
        // Restrict
        protected override List<ReportColumnMapping> RestrictColumns(MappedSearchRequest request)
        {
            using (new DebugTimer("DateStatsCteQueryBuilder.Build"))
            {
                var selectedColumnsPlusFilters = request.SelectedAndDependantColumns.ToList();

                // remove NON stats columns, action stats columns & calculated stats columns
                var result = selectedColumnsPlusFilters.Where(IsRawStatColumn).ToList();

                foreach (var col in _foreignKeyColumnIds)
                {
                    if (result.All(x => x.Id != col))
                    {
                        result.Add(QueryHelpers.GetColumnMapping(col));
                    }
                }

                AddColumnsForGroupBy(request, result);

                if (result.All(x => x.Id != _constants.DateStatColumnId))
                {
                    result.Add(QueryHelpers.GetColumnMapping(_constants.DateStatColumnId));
                }

                var currency = GetCurrencyColumn();
                if (currency != null)
                {
                    result.Add(currency);
                }

                return result;
            }
        }
        
        protected virtual void AddColumnsForGroupBy(MappedSearchRequest request, List<ReportColumnMapping> result)
        {
            var column = RestrictGroupBy(request);
            if (result.All(x => x.Id != column.Id))
            {
                result.Add(column);
            }
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

            var groupByColumn = _columnProvider.Find(_constants.DataSourceId, _statsTableName, statForeignKeyToDataTable, null).First();
            return groupByColumn;
        }
   
        protected override ReportColumnMapping RestrictSort(MappedSearchRequest request, bool forOrderBy = false)
        {
            if (request.SortByColumn != null)
            {
                return request.SortByColumn.Id > 0
                            && IsRawStatColumn(request.SortByColumn)
                        ? request.SortByColumn : null;
            }
            return null;
        }

        protected virtual bool IsRawStatColumn(ReportColumnMapping column)
        {
            return QueryHelpers.IsStatsColumn(column) // exclude data columns
                   && !QueryHelpers.IsCalculatedColumn(column) // exclude it if its a calculated column, calculations are done in StatsQueryBuilder
                   && !(QueryHelpers).IsTransposeStatColumn(column); // exclude it if its from the transpose stats table
        }
        
        // Override
        protected override string GetFieldAlias(SelectedColumn column)
        {
            return QueryHelpers.GetFieldName(column);
        }

        protected override string GetFieldAlias(ReportColumnMapping column, MappedSearchRequest request)
        {
            var result = QueryHelpers.GetFieldName(column);

            // to allow selecting both min and max together (not working with custom columns - its complicated)
            // depends on DefaultStatsQueryBuilder.GetColumnSelector()
//            if (RestrictGroupBy(request).Id != column.Id && result != _constants.CountKeyAlias)
//            {
//                result += "_" + column.FieldAggregationMethod;
//            }
            return result;
        }
         
        protected override void AddTemporalAggregationGroupBy(SelectQuery query, TemporalAggregation temporalAggregation)
        {
            StatsQueryHelpers.AddTemporalAggregationGroupBy(query, temporalAggregation, _constants.StatsDateDbField);
        }

        public override ReportColumnMapping GetCurrencyColumn()
        {
            return _dataSourceComponents.StatsQueryBuilder.GetCurrencyColumn();
        }

        public override bool IsDateColumn(ReportColumnMapping x)
        {
            return _dataSourceComponents.StatsQueryBuilder.IsDateColumn(x);
        }

        public override bool RequireCurrencyGroupBy(MappedSearchRequest request)
        {
            return _dataSourceComponents.StatsQueryBuilder.RequireCurrencyGroupBy(request);
        }

    }
}
