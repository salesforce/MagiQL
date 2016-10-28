using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;
using SqlModeller.Model;
using SqlModeller.Shorthand;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Stats
{
    public class DefaultStatsQueryBuilder : QueryBuilderBase
    {
        protected string _statsTableName; 
        protected string _statsTableAlias;
        protected readonly string _dateStatsTableAlias;
        protected readonly string _transposeStatsTableAlias;
        protected readonly string _transposeStatsKeyField;

        protected IDataSourceComponents _dataSourceComponents;
        
        public DefaultStatsQueryBuilder(IDataSourceComponents dataSourceComponents) : base(dataSourceComponents)
        {
            _dataSourceComponents = dataSourceComponents; 

            var statsTable = dataSourceComponents.TableMappings.GetAllTables().FirstOrDefault(x => x is StatsTableMapping);
            if (statsTable != null)
            {
                _statsTableName = statsTable.KnownTableName;
                _statsTableAlias = statsTable.Alias;
                _dateStatsTableAlias = "dateStats"; // _constants.DateStatsTableAliasPrefix + _statsTableAlias;
            }

            var transposeStatsTable = dataSourceComponents.TableMappings.GetAllTables().FirstOrDefault(x => x is TransposeStatsTableMapping);
            if (transposeStatsTable != null)
            {
                _transposeStatsTableAlias = transposeStatsTable.Alias;
                _transposeStatsKeyField = "_" + ((TransposeStatsTableMapping)transposeStatsTable).TransposeKey;
            }  
        }
        
        protected override List<CommonTableExpression> BuildCommonTableExpressions(MappedSearchRequest request)
        {

            var result = new List<CommonTableExpression>();

            // build the date stats CTE
            if (IsDateQuery(request))
            {
                using (new DebugTimer("StatsQueryBuilder.BuildCommonTableExpressions - date"))
                {
                    var dateStatsQuery = _dataSourceComponents.DateStatsQueryBuilder.Build(request);

                    var cte = new CommonTableExpression
                    {
                        Alias =  _dateStatsTableAlias,
                        Query = dateStatsQuery.SelectQuery
                    };
                    result.Add(cte);
                }
            }


            // bulid the transpose stats CTE
            var allTransposeStatsColumns = GetAllTransposeStatsColumns(request);

            if (allTransposeStatsColumns != null && allTransposeStatsColumns.Any())
            {
                using (new DebugTimer("StatsQueryBuilder.BuildCommonTableExpressions - transpose"))
                {
                    var transposeCteQuery = _dataSourceComponents.TransposeStatsQueryBuilder.Build(request);

                    var transposeStatsCte = new CommonTableExpression
                    {
                        Alias = _transposeStatsTableAlias,
                        Query = transposeCteQuery.SelectQuery
                    };

                    result.Add(transposeStatsCte);
                }
            }

            return result;
        }

        public void AddTransposeStatsJoins(SelectQuery query, TemporalAggregation resolution, IEnumerable<int?> distinctTransposeKeys, string joinCol)
        {
            foreach (var transposeKey in distinctTransposeKeys)
            {
                string joinTableAlias = _transposeStatsTableAlias + "_" + transposeKey;

                string transposeStatJoin = string.Format(" AND {0}.{1} = {2}", joinTableAlias, _transposeStatsKeyField, transposeKey);

                string dateKeyJoin = null;
                if (resolution != TemporalAggregation.Total)
                {
                    dateKeyJoin = string.Format(" AND {0}.{2} = {1}.{2}", joinTableAlias, _statsTableAlias, _constants.DateKeyAlias);
                }

                // join the transpose stats table to the stats table using the transpose key
                query.LeftJoin(_transposeStatsTableAlias, joinTableAlias, _constants.GroupKeyAlias, _statsTableAlias, joinCol, transposeStatJoin + dateKeyJoin);
            }
        }

        public List<ReportColumnMapping> GetAllTransposeStatsColumns(MappedSearchRequest request)
        {
            var allTransposeStatsColumns = request.SelectedAndDependantColumns
                .Where(x => QueryHelpers.IsTransposeStatColumn(x))
                .ToList();

            return allTransposeStatsColumns;
        }

//        public override Column GetColumnSelector(ReportColumnMapping col, MappedSearchRequest request, bool dontAggregate = false, bool useFieldAlias = false)
//        {

//            var result = base.GetColumnSelector(col, request, dontAggregate: dontAggregate, useFieldAlias: useFieldAlias);

        // to allow selecting both min and max together (not working with custom columns - its complicated)
        // depends on DefaultStatsQueryBuilder.GetColumnSelector()
//           if (IsDateQuery(request) && (result.TableAlias??"").ToLower() == "stats" && !dontAggregate && result.Field.Name != _constants.CountKeyAlias)
//            {
//                result.Field.Name += "_" + col.FieldAggregationMethod;
//            }
//            return result;
//        }

        public override void BuildFrom(SelectQuery query, List<ReportColumnMapping> selectedColumns, ReportColumnMapping groupByColumn, ReportColumnMapping sortByColumn, MappedSearchRequest request)
        {

            using (new DebugTimer("StatsQueryBuilder.BuildFrom"))
            {
                var graphBuilder = new TableRelationshipGraphBuilder();

                var tables = request.SelectedAndDependantColumns.Select(x => x.KnownTable).Distinct().ToList();

                tables.Add(groupByColumn.KnownTable);
                tables = tables.Distinct().ToList();

                var statsRelationships = _tableMappings.GetAllTableRelationships()
                        .Where(x => x.Table1 is StatsTableMapping || x.Table2 is StatsTableMapping)
                        .ToList();

                // get the data table which joins onto the stats table 
                var dataJoinTable = GetDataJoinTable(groupByColumn, statsRelationships, graphBuilder);

                // get the stats relationship to the data join table
                var statsRelationship = statsRelationships.FirstOrDefault(x => x.Table1.KnownTableName == dataJoinTable || x.Table2.KnownTableName == dataJoinTable);

                string joinCol;
                if (statsRelationship != null)
                {
                    // var dataTable = statsRelationship.Table1.KnownTableName == groupByColumn.KnownTable//                       
                    var dataTable = statsRelationship.Table1.TableType == TableType.Data
                        ? statsRelationship.Table1.KnownTableName
                        : statsRelationship.Table2.KnownTableName;

                    //var statForeignKeyToDataTable = statsRelationship.Table1.KnownTableName == groupByColumn.KnownTable
                    var statForeignKeyToDataTable = statsRelationship.Table1.TableType == TableType.Data
                        ? statsRelationship.Table2Column
                        : statsRelationship.Table1Column;

                    query.From(GetStatsTableName(dataTable, request), _statsTableAlias);
                    JoinTables(query, dataTable, _statsTableName);
                    joinCol = statForeignKeyToDataTable;


                    var addedTables = new Dictionary<string, bool> { { dataTable, false } };

                    // join back up to the root table  todo : exclude tables not used for filtering or calculations
                    AddDataTableJoinsUpToRoot(query, dataTable, graphBuilder, addedTables);


                    // add data table joins : todo : exclude tables not used for filtering or calculations  
                    AddMissingDataTables(query, groupByColumn, graphBuilder, tables, _dataSourceComponents.QueryBuilderBase, addedTables);

                    var currencyColumn = GetCurrencyColumn();
                    if (currencyColumn != null && !addedTables.ContainsKey(currencyColumn.KnownTable))
                    {
                        var relationshipGraph = graphBuilder.Build(_tableMappings.GetAllTableRelationships(), _constants.RootTableName);
                        AddFromTableJoin(query, groupByColumn, currencyColumn.KnownTable, ref addedTables, graphBuilder, relationshipGraph);
                    }
                }
                else
                {
                    throw new Exception("No Relationship found to join the Stats table to " + groupByColumn.KnownTable);
                }

                var resolution = RestrictResolution(request);
                var allTransposeStatsColumns = GetAllTransposeStatsColumns(request);
                var distinctTransposeKeys = allTransposeStatsColumns.Select(x => QueryHelpers.GetTransposeKeyValue(x)).Distinct();

                AddTransposeStatsJoins(query, resolution, distinctTransposeKeys, joinCol);
            }
        }
         
        // Restrict
        protected override List<ReportColumnMapping> RestrictColumns(MappedSearchRequest request)
        {
            var result = request.SelectedColumns.ToList();
            result.AddRange(request.DependantColumns);

            result = result.Where(x =>
                QueryHelpers.IsStatsColumn(x) // remove data columns
                ).ToList();

            if (request.SummarizeByColumn != null)
            {
                //todo : exclude calculated columns - they will be calculated in the searchQueryBuilder
            }

            // remove date columns from the select
            result = RemoveDateColumnsIfNonDateQuery(request, result);

            return result.ToList();
        }
        
        protected override List<MappedSearchRequestFilter> RestrictFilters(MappedSearchRequest request, bool isWhere)
        {
            if (request.Filters == null || !request.Filters.Any())
            {
                return request.Filters;
            }

            // remove aggregated columns for where filters and non aggregated columns for having filters
            var result = request.Filters.Where(x => x.ProcessBeforeAggregation == isWhere);

            if (!isWhere)
            {
                result = result.Where(x => !x.Column.IsCalculated);
            }

            return result.ToList();
        } 

        protected override ReportColumnMapping RestrictSort(MappedSearchRequest request, bool forOrderBy = false)
        {
            if (forOrderBy)
            {
                return request.SortByColumn;
            }

            if (request.SortByColumn != null)
            {
                // remove data columns
                return request.SortByColumn.Id > 0 && QueryHelpers.IsStatsColumn(request.SortByColumn) ? request.SortByColumn : null;
            }
            return null;
        }
        
        public virtual string GetStatsTableName(string groupTable, MappedSearchRequest request)
        {
            // Do we need to query the daily or hourly tables?
            if (IsDateQuery(request))
            {
                // This request requires us to query the daily or hourly tables. The DateStatsCteQueryBuilder
                // will take care of building a CTE that does that and will call it "dateStats". It's the view 
                // that will contain the stats.
                return "dateStats";
            }

            // We don't need to query the daily or hourly stats tables. I.e. this is a 
            // straighforward request to query lifetime stats from the lifetime stats tables. 

            // Get the name of the Lifetime stats table we need to query. If we've been asked to return stats data
            // grouped by AdGroup, we'll have to query the AdGroupLifetime table. But for all other groupings,
            // we can get the data we need from the CampaignLifetime table.
            return GetStatsTableName(_statsTableName, groupTable, request.TemporalAggregation, request.DateRangeType, request.DateStart, request.DateStart);
        }
        
        public override bool IsDateColumn(ReportColumnMapping x)
        {
            return x.Id == _constants.DateStatColumnId || x.Id == _constants.HourStatColumnId;
        }
         
        protected override bool CanFilterOnText()
        {
            // when using contains, its a lot slower to do this in the stats cte as well as the data cte
            return false;
        }

        public override void SelectGroupKey(SelectQuery query, MappedSearchRequest request)
        {
            if (QueryHelpers.IsStatsColumn(request.GroupByColumn))
            {
                // we cannot group by a stats column, so we need to group by the column stats join onto

                // get the stats relationship to the data join table
                var statsRelationship =
                    _tableMappings.GetAllTableRelationships()
                        .FirstOrDefault(x => x.Table1 is StatsTableMapping || x.Table2 is StatsTableMapping);

                if (statsRelationship != null)
                {
                    var dataTable = statsRelationship.Table1.TableType == TableType.Data
                        ? statsRelationship.Table1
                        : statsRelationship.Table2;

                    var primaryKeyColumn = _columnProvider.Find(_constants.DataSourceId, dataTable.KnownTableName,
                        dataTable.PrimaryKey, null);
                    if (primaryKeyColumn != null && primaryKeyColumn.Count == 1)
                    {
                        var groupCol = primaryKeyColumn.First();
                        query.Select(dataTable.Alias, groupCol.FieldName, _constants.GroupKeyAlias, Aggregate.Min);
                    }
                }
            }
            else
            {
                base.SelectGroupKey(query, request);
            }
        }

        public override ReportColumnMapping GetCurrencyColumn()
        {
            return _columnProvider.GetColumnMapping(_constants.DataSourceId, _constants.CurrencyColumnId);
        }
        
        public virtual string GetDataJoinTable(ReportColumnMapping groupByColumn, List<TableRelationship> statsRelationships, TableRelationshipGraphBuilder graphBuilder)
        {
            var statsJoinTables = statsRelationships
                .Select(x => x.Table1 is StatsTableMapping ? x.Table2.KnownTableName : x.Table1.KnownTableName)
                .ToList();

            // find the nearest stats table to the group by table.
            var graph = graphBuilder.Build(_tableMappings.GetAllTableRelationships(), groupByColumn.KnownTable);

            graphBuilder.TrimToTables(graph, statsJoinTables);
            var dataTables = graphBuilder.GetAllTables(graph).Distinct().ToList();

            dataTables = dataTables.OrderBy(x => graphBuilder.GetDistance(graph, x)).ToList();

            var joinTable = dataTables.FirstOrDefault(statsJoinTables.Contains);
            return joinTable;
        }

        protected void AddMissingDataTables(
         SelectQuery query,
         ReportColumnMapping groupByColumn,
         TableRelationshipGraphBuilder graphBuilder,
         List<string> tables,
         QueryBuilderBase queryBuilderBase,
         Dictionary<string, bool> addedTables)
        {
            // build a graph of the table relationships to calculate the distance from the root table
            // include missing tables needed for joins
            // order the tables by distance
            var relationshipGraph = graphBuilder.Build(_tableMappings.GetAllTableRelationships(), _constants.RootTableName);
            graphBuilder.TrimToTables(relationshipGraph, tables);
            tables = graphBuilder.GetAllTables(relationshipGraph).Union(tables).Distinct().ToList();

            tables = tables.OrderBy(x => graphBuilder.GetDistance(relationshipGraph, x)).ToList();

            foreach (var table in tables.Distinct())
            {
                queryBuilderBase.AddFromTableJoin(query, groupByColumn, table, ref addedTables, graphBuilder, relationshipGraph);
            }
        }

        public void AddDataTableJoinsUpToRoot(
            SelectQuery query,
            string dataTable,
            TableRelationshipGraphBuilder graphBuilder,
            Dictionary<string, bool> addedTables)
        {

            if (dataTable != _constants.RootTableName)
            {
                var fromTable = _tableMappings.GetTableMapping(dataTable);
                var relationshipGraph = graphBuilder.Build(_tableMappings.GetAllTableRelationships(), _constants.RootTableName);

                var path = graphBuilder.GetPathFromTableToRoot(relationshipGraph, dataTable);
                string currentTableName = fromTable.KnownTableName;
                foreach (var table in path.Skip(1))
                {
                    if (!addedTables.ContainsKey(table))
                    {
                        JoinTables(query, table, currentTableName);
                        addedTables.Add(table, false);
                        currentTableName = table;
                    }
                }
            }
        }
        
        public override bool RequireCurrencyGroupBy(MappedSearchRequest request)
        {
            return _dataSourceComponents.QueryBuilderBase.RequireCurrencyGroupBy(request);
        }

        public override bool GroupByIsChildTable(string tableAlias, ReportColumnMapping groupByColumn)
        {
            return _dataSourceComponents.QueryBuilderBase.GroupByIsChildTable(tableAlias, groupByColumn);
        }
        
        public override bool WhereFilterColumnAllowIsNull(MappedSearchRequestFilter filter)
        {
            return _dataSourceComponents.QueryBuilderBase.WhereFilterColumnAllowIsNull(filter);
        }
    }
}
