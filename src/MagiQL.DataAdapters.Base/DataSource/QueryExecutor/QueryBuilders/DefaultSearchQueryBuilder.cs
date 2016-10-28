using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Functions;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model.Columns;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;
using SqlModeller.Model;
using SqlModeller.Model.From;
using SqlModeller.Shorthand;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders
{
    public class DefaultSearchQueryBuilder : QueryBuilderBase
    {
        protected readonly IDataSourceComponents _dataSourceComponents;

        public DefaultSearchQueryBuilder(IDataSourceComponents dataSourceComponents) : base(dataSourceComponents)
        {
            _dataSourceComponents = dataSourceComponents;
        }
        
        protected virtual void RemoveUnusedCommonTableExpressions(Query result, Dictionary<string, CommonTableExpression> cteDict)
        {  
            if (cteDict != null && cteDict.Any())
            {
                foreach (var cteKey in cteDict.Values.Select(x=>x.Alias))
                {
                    if (cteKey == "missingSummarizeData")
                    {
                        continue;
                    }

                    if (
                        // cte is not used in nested CTE queries
                        result.CommonTableExpressions.All(x => x.Query.FromTable.TableName != cteKey && x.Query.TableJoins.All(y => ((TableJoin)y).JoinTable.TableName != cteKey)
                        // cte is not used in the main query
                        && result.SelectQuery.FromTable.TableName != cteKey && result.SelectQuery.FromTable.Alias != cteKey && result.SelectQuery.TableJoins.All(y => ((TableJoin)y).JoinTable.TableName != cteKey))
                        )
                    {
                        var cte = result.CommonTableExpressions.FirstOrDefault(x => x.Alias == cteKey);
                        if (cte != null)
                        {
                            result.CommonTableExpressions.Remove(cte);
                        }
                    }
                }
            } 
        }

        // Build
        public override Query Build(MappedSearchRequest request)
        {
            var result = new Query();

            using (new DebugTimer("SearchQueryBuilder.Build"))
            { 
                Dictionary<string, CommonTableExpression> cteDict;
                AddCommonTableExpressions(request, result, out cteDict);

                var selectQuery = new SelectQuery();
                result.SelectQuery = selectQuery;

                var restrictedColumns = RestrictColumns(request);

                BuildSelect(selectQuery, restrictedColumns, RestrictSort(request), RestrictGroupBy(request), request);
                BuildFrom(selectQuery, restrictedColumns, request.GroupByColumn, request.SortByColumn, request);
                BuildGroupBy(selectQuery, RestrictGroupBy(request), request);
                BuildWhere(selectQuery, request.TextFilter, RestrictFilters(request, isWhere: true), request);
                BuildHaving(selectQuery, RestrictFilters(request, isWhere: false), request.GroupByColumn, request);
                BuildOrderBy(selectQuery, RestrictSort(request, forOrderBy: true), request);
                BuildPaging(selectQuery, request);

                RemoveUnusedCommonTableExpressions(result, cteDict);

                AddParameters(result);
            }

            return result;
        }

        protected virtual void AddParameters(Query result){}

        public override void BuildSelect(SelectQuery query, List<ReportColumnMapping> selectedColumns, ReportColumnMapping sortColumn, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            // this was the old way of doing it, when the row number was calculated in the CTE, but had performance issues
            //            var row = request.SummarizeByColumn == null ? "_R" : "MIN(_R)";
            //            query.Select(string.Format("ISNULL({0}, 2147483647) as _ROW", row));
            //            if (request.GetCount)
            //            {
            //                query.Select(string.Format("CASE WHEN ROW_NUMBER() OVER(ORDER BY ISNULL({0}, 2147483647) ASC) = {1} THEN COUNT(1) OVER () ELSE 0 END AS _COUNT", row, (request.PageIndex * request.PageSize) + 1));
            //            }

            var sortField = "_SortKey";
            var row = request.SummarizeByColumn == null ? sortField : "MIN("+sortField+")";
            var rowNumberSelect = string.Format("ROW_NUMBER() OVER(ORDER BY ISNULL({0}, {2}) {1})", 
                row, 
                request.SortDescending ? "DESC" : "ASC", 
                request.SortDescending ? request.SortByColumn.DbType.MinValue() : request.SortByColumn.DbType.MaxValue()
                );
           
            query.Select(rowNumberSelect + " as _ROW");
            if (request.GetCount)
            {
                query.Select("COUNT(1) OVER () AS _COUNT");
            }


            foreach (var col in selectedColumns)
            {

                var aggregate = Aggregate.None; 
                var columnSelector = GetFieldAlias(col, request);

                bool isCustomColumn = col.OrganizationId > 0;
                bool isSummarizing = request.SummarizeByColumn != null;

                // need to calculate custom columns here, because there may be no stats
                if (isSummarizing || isCustomColumn)
                {
                    aggregate = MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.GetAggregate(col.FieldAggregationMethod);

                    if (col.IsCalculated && col.FieldAggregationMethod == FieldAggregationMethod.Average)
                    {
                        // when summarizing, the calculated columns need to be calculated from aggregate values
                        var colName = GetColumnSelector(col, request, dontAggregate: !isSummarizing, useFieldAlias: true);
                        columnSelector = colName.Field.Name;
                    }

                    if (QueryHelpers.ContainsAggregate(columnSelector) || !isSummarizing)
                    {
                        aggregate = Aggregate.None;
                    }


                    if (isSummarizing && aggregate == Aggregate.Sum)
                    {
                        if (col.DbType == DbType.Int32 || col.DbType == DbType.Int16)
                        {
                            columnSelector = new ToBigIntFunction().Parse(columnSelector);
                        }
                    }
                }


                query.Select(string.Empty, columnSelector, GetFieldAlias(col, request), aggregate);
            }
        }

        public override void BuildFrom(SelectQuery query, List<ReportColumnMapping> selectedColumns, ReportColumnMapping groupByColumn, ReportColumnMapping sortByColumn, MappedSearchRequest request)
        {
            // All we're doing here is join the CTE containting the "data" (i.e. the list of either campaigns, ad groups or ad sets that 
            // we've been asked to query) with the CTE containing their stats. If a given "data" record (e.g. a campaign) hasn't had any stats,
            // the "stats" CTE won't have any record for it. So we can easily exclude or include data records that don't have any stats by 
            // doing respectively either an INNER JOIN or a LEFT OUTER JOIN on the stats table.
            var statsTableJoinType = request.ExcludeRecordsWithNoStats ? JoinType.InnerJoin : JoinType.LeftJoin;

            if (request.SummarizeByColumn != null && _dataSourceComponents.MissingSummarizeDataQueryBuilder.IncludeInQuery(request))
            {
                // union data with missingSummarizeData 
                query.From("(select * from data UNION select * from missingSummarizeData)", "data");
            }
            else
            {
                query.From("data", "data");
            }



            if (  selectedColumns.Any(x=>QueryHelpers.IsStatsColumn(x)) 
                || request.DependantColumns.Any(x=>QueryHelpers.IsStatsColumn(x)))
            {
                var extra = "";
                if (_dataSourceComponents.QueryBuilderBase.RequireCurrencyGroupBy(request))
                {
                    string.Format("AND {0}.{2} = {1}.{2}", "data", "stats", _constants.CurrencyKeyAlias);
                }
                query.Join("stats", "stats", _constants.GroupKeyAlias, "data", _constants.GroupKeyAlias, statsTableJoinType, extra);
            }
        }

        protected override void BuildWhere(SelectQuery query, string queryText, List<MappedSearchRequestFilter> filters, MappedSearchRequest request)
        {
            var filterCollection = BuildWhereFilters(request.Filters, request, true, true);
            query.WhereFilters = filterCollection;
        }

        protected override void BuildGroupBy(SelectQuery query, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            if (request.SummarizeByColumn != null)
            {
                query.GroupBy(string.Empty, GetFieldAlias(request.SummarizeByColumn, request));
            }
        }

        protected override void BuildHaving(SelectQuery query, List<MappedSearchRequestFilter> filters, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            // not allowed
        }

        protected override void BuildOrderBy(SelectQuery query, ReportColumnMapping sortColumn, MappedSearchRequest request)
        {
            bool isSummarizing = request.SummarizeByColumn != null;
            if (isSummarizing)
            {
                var dir = request.SortDescending ? OrderDir.Desc : OrderDir.Asc;
                query.OrderBy(string.Empty, _dataSourceComponents.QueryHelpers.GetFieldAlias(request.SortByColumn), dir);
            }
            else
            {
                query.OrderBy(string.Empty, "_ROW");
            }
        }

        protected override void BuildPaging(SelectQuery query, MappedSearchRequest request)
        {
            query
                .Offset(request.PageIndex * request.PageSize)
                .Fetch(request.PageSize);
        }
        
        // Restrict
        protected override List<ReportColumnMapping> RestrictColumns(MappedSearchRequest request)
        {
            var result = base.RestrictColumns(request);
            result = RemoveDateColumnsIfNonDateQuery(request, result);
            return result;
        }

        // Override
        public override ReportColumnMapping GetCurrencyColumn()
        {
            return null;
        } 
         
        protected virtual void AddCommonTableExpressions(
            MappedSearchRequest request,
            Query result,
            out Dictionary<string, CommonTableExpression> cteDict)
        {
            cteDict = new Dictionary<string, CommonTableExpression>();

            var cteTables =
                  _tableMappings.GetAllTableRelationships()
                      .Where(x => x.IsDirect && x.RelationshipType == TableRelationshipType.OneToMany || x.RelationshipType == TableRelationshipType.ManyToOne)
                      .Select(x => x.Table2.KnownTableName)
                  .Union(_tableMappings.GetAllTableRelationships()
                      .Where(x => x.IsDirect && x.RelationshipType == TableRelationshipType.OneToMany || x.RelationshipType == TableRelationshipType.ManyToOne)
                      .Select(x => x.Table1.KnownTableName)
                  )
                  .Distinct()
                  .ToList();

            var siblings = _tableMappings.GetAllTableRelationships()
                .Where(x => x.RelationshipType == TableRelationshipType.OneToOne && cteTables.Contains(x.Table1.KnownTableName))
                .Select(x => x.Table2.KnownTableName)
                .Union(_tableMappings.GetAllTableRelationships()
                    .Where(x => x.RelationshipType == TableRelationshipType.OneToOne && cteTables.Contains(x.Table2.KnownTableName))
                    .Select(x => x.Table1.KnownTableName)
                ).Distinct().ToList();

            cteTables = cteTables.Union(siblings).Distinct().ToList();

            // todo : may be able to optimise this by not building tables which are not needed and later removed
            foreach (var knownTable in cteTables)
            {
                var table = _tableMappings.GetTableMapping(knownTable);

                if (table == null)
                {
                    throw new Exception(string.Format("Could not find '{0}' table in table mappings", knownTable));
                }

                if (table.TableType == TableType.Stats)
                {
                    continue;
                }

                using (new DebugTimer("CteQueryBuilder.Build - " + table.CteAlias))
                {
                    var cteBuilder = _dataSourceComponents.OneToManyCteQueryBuliderFactory.Create(table.KnownTableName);
                    var cteQuery = cteBuilder.Build(request);
                    var cte = new CommonTableExpression { Alias = table.CteAlias, Query = cteQuery.SelectQuery };
                    result.CommonTableExpressions.Add(cte);
                    cteDict.Add(table.Alias, cte);
                }
            }



            using (new DebugTimer("CteQueryBuilder.Build - dataCte"))
            {
                var dataQuery = _dataSourceComponents.DataQueryBuilder.Build(request);
                var dataCte = new CommonTableExpression { Alias = "data", Query = dataQuery.SelectQuery };
                result.CommonTableExpressions.Add(dataCte);
                cteDict.Add("data", dataCte);
            }


            if (request.SummarizeByColumn != null && _dataSourceComponents.MissingSummarizeDataQueryBuilder.IncludeInQuery(request))
            {
                using (new DebugTimer("CteQueryBuilder.Build - missingSummarizeDataCte"))
                {
                    var dataQuery = _dataSourceComponents.MissingSummarizeDataQueryBuilder.Build(request);
                    var dataCte = new CommonTableExpression {Alias = "missingSummarizeData", Query = dataQuery.SelectQuery};
                    result.CommonTableExpressions.Add(dataCte);
                    cteDict.Add("missingSummarizeData", dataCte);
                }
            }

            if (_dataSourceComponents.TableMappings.GetAllTables().Any(x => x.TableType == TableType.Stats))
            { 
                using (new DebugTimer("CteQueryBuilder.Build - statsCte"))
                {
                    var statsQuery = _dataSourceComponents.StatsQueryBuilder.Build(request);
                    var statsCte = new CommonTableExpression {Alias = "stats", Query = statsQuery.SelectQuery};
                    result.CommonTableExpressions.AddRange(statsQuery.CommonTableExpressions);
                    result.CommonTableExpressions.Add(statsCte);
                    cteDict.Add("statsCte", statsCte);
                }
            }
        }
        
        public override bool GroupByIsChildTable(string tableAlias, ReportColumnMapping groupByColumn)
        {
            return _dataSourceComponents.QueryBuilderBase.GroupByIsChildTable(tableAlias, groupByColumn);
        }

        public override bool IsDateColumn(ReportColumnMapping x)
        {
            return x.Id == _constants.DateStatColumnId || x.Id == _constants.HourStatColumnId;
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
