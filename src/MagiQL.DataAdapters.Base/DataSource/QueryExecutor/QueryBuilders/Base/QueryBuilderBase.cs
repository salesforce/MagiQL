using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;
using SqlModeller.Model;
using SqlModeller.Model.Having;
using SqlModeller.Model.Where;
using SqlModeller.Shorthand;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base
{
    public abstract class QueryBuilderBase
    {
        private readonly IDataSourceComponents _dataSourceComponents;

        protected DefaultQueryHelpers QueryHelpers { get { return _dataSourceComponents.QueryHelpers; } }
        protected DefaultCalculatedColumnHelper _calculatedColumnHelper { get { return _dataSourceComponents.CalculatedColumnHelper; } }
        protected IColumnProvider _columnProvider { get { return _dataSourceComponents.ColumnProvider; } }
        protected ConstantsBase _constants { get { return _dataSourceComponents.Constants; } }
        protected TableMappingsBase _tableMappings { get { return _dataSourceComponents.TableMappings; } }

        protected QueryBuilderBase(IDataSourceComponents dataSourceComponents)
        {
            _dataSourceComponents = dataSourceComponents;  
        }
         
        public virtual ReportColumnMapping GetCurrencyColumn()
        {
            return _dataSourceComponents.StatsQueryBuilder.GetCurrencyColumn();
        }
        
        // CTE
        protected virtual List<CommonTableExpression> BuildCommonTableExpressions(MappedSearchRequest request)
        {
            return new List<CommonTableExpression>();
        }
         
        // Build
        public virtual Query Build(MappedSearchRequest request)
        {
            var query = new SelectQuery();
            var result = new Query
            {
                SelectQuery = query
            };

            var restrictedColumns = RestrictColumns(request);

            BuildSelect(query, restrictedColumns, RestrictSort(request), RestrictGroupBy(request), request);
            BuildFrom(query, restrictedColumns, request.GroupByColumn, request.SortByColumn, request);
            BuildGroupBy(query, RestrictGroupBy(request), request);
            BuildWhere(query, request.TextFilter, RestrictFilters(request, isWhere: true), request);
            BuildHaving(query, RestrictFilters(request, isWhere: false), request.GroupByColumn, request);
            BuildOrderBy(query, RestrictSort(request, forOrderBy: true), request);
            BuildPaging(query, request);

            result.CommonTableExpressions = BuildCommonTableExpressions(request);

            return result; 
        }

        public virtual void BuildSelect(SelectQuery query, List<ReportColumnMapping> selectedColumns, ReportColumnMapping sortColumn, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        { 
            var resolution = RestrictResolution(request);

            using (new DebugTimer("SearchQueryBuilder.BuildSelect (" + selectedColumns.Count + ")"))
            {
                query.SelectCount(_constants.CountKeyAlias);
                SelectGroupKey(query, request);

                if (resolution != TemporalAggregation.Total)
                {
                    query.SelectGroupKey(_constants.DateKeyAlias, 1 + (RequireCurrencyGroupBy(request) ? 1 : 0));
                }

                if (sortColumn != null && CanSelectRowNumber())
                {
                    //query.SelectRowNumber("_R"); // this can be slow inside the CTE
                    query.SelectOrderByColumn(_constants.SortKeyAlias); // so just get the value in a known column and get rownumber later
                }
                 
                var currencyColumn = GetCurrencyColumn();
                if (currencyColumn != null)
                {
                    var currencyCol = GetColumnSelector(currencyColumn, request);
                    var aggregate = RequireCurrencyGroupBy(request) ? Aggregate.None : Aggregate.Min;
                    query.Select(currencyCol.TableAlias, currencyCol.Field.Name, _constants.CurrencyKeyAlias, aggregate);
                    
                }



                var selectedAliases = new List<string>() { _constants.CountKeyAlias, _constants.GroupKeyAlias };
                foreach (var col in selectedColumns)
                {
                    var colName = GetColumnSelector(col, request);
                   
                    var aggregate = MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.GetAggregate(col.FieldAggregationMethod);

                    if (QueryHelpers.ContainsAggregate(colName.Field.Name))
                    {
                        aggregate = Aggregate.None;
                    }

                    var fieldAlias = GetFieldAlias(col, request);

                    // dont request the same column twice
                    if (selectedAliases.Contains(fieldAlias))
                    {
                        continue;
                    } 

                    // theres a special case for handling counts, if the referenced table is not a CTE, then we should actually select 1
                    if (colName.Field.Name == _constants.CountKeyAlias)
                    {
                        HandleSelectCountColumn(request, colName);
                    }
                    
                    query.Select(colName.TableAlias, colName.Field.Name, fieldAlias, aggregate);
                    selectedAliases.Add(fieldAlias);
                }
            }

        }

        public virtual void SelectGroupKey(SelectQuery query, MappedSearchRequest request)
        {
            query.SelectGroupKey(_constants.GroupKeyAlias);
        }

        public virtual bool GroupByIsChildTable(string tableAlias, ReportColumnMapping groupByColumn)
        {
            var table = _tableMappings.GetAllTables().First(x => x.Alias == tableAlias);

            var isChild = TableIsChildOfTable(table.KnownTableName, groupByColumn.KnownTable);

            return isChild;
        }

        public virtual void BuildFrom(
            SelectQuery query,
            List<ReportColumnMapping> selectedColumns,
            ReportColumnMapping groupByColumn,
            ReportColumnMapping sortByColumn,
            MappedSearchRequest request)
        {
            var tables = request.SelectedAndDependantColumns.Select(x => x.KnownTable).ToList();
            tables.Add(groupByColumn.KnownTable);
            tables = tables.Distinct().ToList();

            var currencyColumn = GetCurrencyColumn();
            if (currencyColumn != null && !tables.Contains(currencyColumn.KnownTable))
            { 
                tables.Add(currencyColumn.KnownTable);   
            }
             
            var rootTableMapping = _tableMappings.GetTableMapping(_constants.RootTableName);

            query.From(rootTableMapping.DbTableName, rootTableMapping.Alias);
             
            var graphBuilder = new TableRelationshipGraphBuilder();
            // bulild a graph of the table relationships to calculate the distance from the root table
            // include missing tables needed for joins
            // order the tables by distance
            var relationshipGraph = graphBuilder.Build(_tableMappings.GetAllTableRelationships(), _constants.RootTableName);
            graphBuilder.TrimToTables(relationshipGraph, tables);
            tables = graphBuilder.GetAllTables(relationshipGraph).Union(tables).Distinct().ToList();

            tables = tables.OrderBy(x => graphBuilder.GetDistance(relationshipGraph, x)).ToList();

            // dictionary of TableName, IsAggregated for tables which have been added to the query
            var addedTables = new Dictionary<string, bool> { { _constants.RootTableName, false } };
            foreach (var table in tables.Distinct())
            {
                AddFromTableJoin(query, groupByColumn, table, ref addedTables, graphBuilder, relationshipGraph);
            }
        }
        
        public virtual void AddFromTableJoin(
            SelectQuery query,
            ReportColumnMapping groupByColumn,
            string tableName,
            ref Dictionary<string, bool> addedTables,
            TableRelationshipGraphBuilder graphBuilder,
            TableRelationshipGraph relationshipGraph)
        {
            var joinTable = _tableMappings.GetTableMapping(tableName);

            if (joinTable.TableType == TableType.Stats)
            {
                return;
            }

            bool isAggregated = false;

            // tables with no child relationships
            if (!addedTables.ContainsKey(tableName))
            {
                var distanceToRootTable = graphBuilder.GetDistance(relationshipGraph, tableName);

                var parentJoin = graphBuilder
                    .GetByDistance(relationshipGraph, distanceToRootTable - 1) // the level above
                    .SingleOrDefault(x => x.Relations.Any(y => y.TableName == tableName));

                if (parentJoin != null)
                {
                    var relation = parentJoin.Relations.First(x => x.TableName == tableName);

                    if (relation.Type == TableRelationshipGraphType.Sibling)
                    {
                        //var parentTable = _tableMappings.GetTableMapping(parentJoin.TableName);

                        var aggregatedTables = addedTables.Where(x => x.Value);
                        var isSiblingOfAggregatedTable = aggregatedTables.Any(x => x.Value && TableIsSiblingOfTable(tableName, x.Key));

                        if (isSiblingOfAggregatedTable)
                        {
                            var parentTable = _tableMappings.GetTableMapping(groupByColumn.KnownTable);

                            // if grouping by the parent of this table, then join on the primary key
                            var joinForeignColumn = parentTable.PrimaryKey;

                            query.LeftJoin(joinTable.CteAlias, joinTable.Alias, _constants.GroupKeyAlias, parentTable.Alias, joinForeignColumn);

                            isAggregated = true;
                        }
                        else
                        {
                            JoinTables(query, tableName, parentJoin.TableName);
                        }
                        addedTables.Add(tableName, isAggregated);
                    }
                    else
                    {
                        var parentTable = _tableMappings.GetTableMapping(parentJoin.TableName);

                        var isChild = TableIsChildOfTable(groupByColumn.KnownTable, joinTable.KnownTableName);
                        var aggregatedViewRequired = !ColumnIsTableOrForeignKeyToTable(groupByColumn, joinTable) && !isChild;

                        if (aggregatedViewRequired)
                        {
                            parentTable = _tableMappings.GetTableMapping(groupByColumn.KnownTable);

                            // if the table being joined cant be connected to the group by table, the CTE will have been grouped by the root table id.
                            if (!graphBuilder.TablesAreInSamePathToRoot(relationshipGraph, tableName, groupByColumn.KnownTable))
                            {
                                parentTable = _tableMappings.GetTableMapping(_constants.RootTableName);
                            }

                            // if grouping by the parent of this table, then join on the primary key
                            var joinForeignColumn = TableIsParentOfTable(groupByColumn.KnownTable, joinTable.KnownTableName) || parentTable.KnownTableName == _constants.RootTableName
                                ? parentTable.PrimaryKey
                                : _constants.GroupKeyAlias;

                            if (addedTables.ContainsKey(parentTable.KnownTableName) && addedTables[parentTable.KnownTableName] == false)
                            {
                                joinForeignColumn = parentTable.PrimaryKey;
                            }

                            query.LeftJoin(joinTable.CteAlias, joinTable.Alias, _constants.GroupKeyAlias, parentTable.Alias, joinForeignColumn);

                            isAggregated = true;
                        }
                        else
                        {
                            JoinTables(query, tableName, parentTable.KnownTableName);
                        }
                        addedTables.Add(tableName, isAggregated);
                    }
                }
                else
                {
                    throw new Exception(string.Format("Could not join table '{0}', no suitable relationship was found", tableName));
                }
            }
        }

        protected virtual void BuildWhere(SelectQuery query, string queryText, List<MappedSearchRequestFilter> filters, MappedSearchRequest request)
        {
            if (filters != null && filters.Any())
            {
                // WHERE
                var filterCollection = BuildWhereFilters(filters, request);
                query.WhereFilters = filterCollection;
            }

            BuildTextSearchWhereFIlters(query, queryText, request);
        }

        private void BuildTextSearchWhereFIlters(
            SelectQuery query,
            string queryText,
            MappedSearchRequest request)
        {
            if (!string.IsNullOrEmpty(queryText) && CanFilterOnText())
            {
                var canUseFullText = false;

                var queryColumns = request.TextFilterColumns;
                if ((queryColumns == null || !queryColumns.Any()) && _constants.TextSearchColumnId > 0)
                {
                    // set queryColumns to the default column
                    var defaultTextSearchColumn = _columnProvider.GetColumnMapping(_constants.DataSourceId, _constants.TextSearchColumnId);
                    queryColumns = new List<ReportColumnMapping>() {defaultTextSearchColumn};
                    canUseFullText = _constants.FullTextSearchEnabled;
                }

                if (!queryColumns.Any())
                {
                    // do nothing
                    return;
                }

                // Add Text Search
                if (queryText.StartsWith("*"))
                {
                    queryText = queryText.Substring(1); // remove the *
                    canUseFullText = false;
                }
                 
                if (canUseFullText)
                {
                    // we are using the default column (and there's only 1)
                    var tableMapping = _tableMappings.GetTableMapping(queryColumns.First().KnownTable);
                    query.WhereColumnContains(tableMapping.Alias, queryColumns.First().FieldName, queryText, ContainsMode.FreeText);
                }
                else
                {
                    var filters = new WhereFilterCollection();

                    foreach (var column in queryColumns)
                    {
                        var tableMapping = _tableMappings.GetTableMapping(column.KnownTable);
                        filters.WhereColumnLike(tableMapping.Alias, column.FieldName, queryText, LikeMode.FreeText);
                    }

                    query.WhereCollection(Combine.Or, filters);
                }
                
            }
        }

        protected virtual void BuildGroupBy(SelectQuery query, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            var resolution = RestrictResolution(request);
            var colName = GetColumnSelector(groupByColumn, request);

            query.GroupBy(colName.TableAlias, colName.Field.Name);

            var currencyColumn = GetCurrencyColumn();
            if (currencyColumn != null && RequireCurrencyGroupBy(request))
            {
                var currencyCol = GetColumnSelector(currencyColumn, request);
                query.GroupBy(currencyCol.TableAlias, currencyCol.Field.Name); 
            }

            if (resolution != TemporalAggregation.Total)
            {
                AddTemporalAggregationGroupBy(query, resolution);
            }
        }

        protected virtual void BuildHaving(SelectQuery query, List<MappedSearchRequestFilter> filters, ReportColumnMapping groupByColumn, MappedSearchRequest request)
        {
            if (filters != null && filters.Any())
            {
                // HAVING
                var filterCollection = BuildHavingFilters(filters, request);

                query.HavingFilters = filterCollection;
            }
        }

        protected virtual void BuildOrderBy(SelectQuery query, ReportColumnMapping sortColumn, MappedSearchRequest request)
        {
            if (sortColumn != null)
            {
                var sortDirection = request.SortDescending ? OrderDir.Desc : OrderDir.Asc;
                var aggregate = MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.GetAggregate(sortColumn.FieldAggregationMethod);
                var sortColName = GetColumnSelector(sortColumn, request);
                if (MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.FieldNameContainsAggregate(sortColName.Field.Name))
                {
                    aggregate = Aggregate.None;
                }


                // theres a special case for handling counts, if the referenced table is not a CTE, then we should actually select 1
                if (sortColName.Field.Name == _constants.CountKeyAlias)
                { 
                    HandleSelectCountColumn(request, sortColName);  
                }


                query.OrderBy(sortColName.TableAlias, sortColName.Field.Name, sortDirection, aggregate);
            }
        }

        protected virtual void BuildPaging(SelectQuery query, MappedSearchRequest request)
        {
        }

        protected virtual void AddTemporalAggregationGroupBy(SelectQuery query, TemporalAggregation temporalAggregation)
        {
            if (temporalAggregation != TemporalAggregation.Total)
            {
                query.GroupBy("stats", _constants.DateKeyAlias); 
            }
        }

        /// <param name="filters">The filters to apply</param>
        /// <param name="isPostFilter">Indicates that the filter is being applied after aggregations, in the final select - it will use column aliases rather than fields</param>
        /// <param name="matchAll">All filters must evaluate to true in order to select the row, effectively an AND operator</param>
        protected WhereFilterCollection BuildWhereFilters(List<MappedSearchRequestFilter> filters, MappedSearchRequest request, bool isPostFilter = false, bool matchAll = true)
        {
            if (filters == null || !filters.Any())
            {
                return new WhereFilterCollection();
            }

            var result = new WhereFilterCollection
            {
                GroupingOperator = matchAll ? Combine.And : Combine.Or
            };


            foreach (var filter in filters)
            {
                if (isPostFilter)
                {
                    if (QueryHelpers.GetTableType(filter.Column.KnownTable) == TableType.Data)
                    {
                        continue; // do not filter on data columns in the main selector
                    }
                }
                else if (QueryHelpers.GetTableType(filter.Column.KnownTable) != TableType.Data)
                {
                    continue;
                }

                var field = GetColumnSelector(filter.Column, request, dontAggregate: !isPostFilter);
                if (isPostFilter)
                {
                    var fieldName = GetFieldAlias(filter.Column, request);
                    field = new Column(string.Empty, fieldName);
                }

                var compareOperator = MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.GetOperator(filter);

                if (filter.Values != null && filter.Values.Count == 1)
                {
                    string isNullValue = null;
                    if (filter.Column.DbType.IsNumericType() && (compareOperator==Compare.Equal || compareOperator==Compare.LessThan || compareOperator == Compare.LessThanOrEqual))
                    {
                        isNullValue = 0.ToString();
                    }
                    
                    // todo : how do we decide which columns should be evaluated with null = 0, and which shouldnt... it has performance impact 
                    // for now just fix this for org id
                    if (!WhereFilterColumnAllowIsNull(filter))
                    {
                        isNullValue = null; // dont allow isnull for org id
                    }
                     

                    result.WhereColumnValue(field.TableAlias, field.Field.Name, compareOperator, filter.Values[0], filter.Column.DbType, filter.Column.UniqueName, isNullValue);
                }
                else
                {
                    result.WhereColumnIn(field.TableAlias, field.Field.Name, filter.Values, filter.Column.DbType, filter.Column.UniqueName); 
                } 
            }

            return result;
        }

        public abstract bool WhereFilterColumnAllowIsNull(MappedSearchRequestFilter filter); 
        
        protected HavingFilterCollection BuildHavingFilters(List<MappedSearchRequestFilter> filters, MappedSearchRequest request, bool isPostFilter = false, bool matchAll = true, bool excludeDataFilters = false)
        {
            if (filters == null || !filters.Any())
            {
                return null;
            }

            var result = new HavingFilterCollection
            {
                GroupingOperator = matchAll ? Combine.And : Combine.Or
            };

            foreach (var filter in filters)
            {
                if (excludeDataFilters && QueryHelpers.GetTableType(filter.Column.KnownTable) == TableType.Data)
                {
                    continue; // dont need to filter out data values - should this be in the CTE builder instead?
                }

                var field = GetColumnSelector(filter.Column, request, dontAggregate: true);
                if (isPostFilter)
                {
                    field = new Column(null, filter.Column.Id.ToString()); // todo : suspect!
                }

                var compareOperator = MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.GetOperator(filter);
                var aggregate = MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.GetAggregate(filter.Column.FieldAggregationMethod);

                if (filter.Values != null && filter.Values.Count == 1)
                {
                    string isNullValue = null;
                    if (filter.Column.DbType.IsNumericType() && (compareOperator == Compare.Equal || compareOperator == Compare.LessThan || compareOperator == Compare.LessThanOrEqual))
                    {
                        isNullValue = 0.ToString();
                    }

                    // theres a special case for handling counts, if the referenced table is not a CTE, then we should actually select 1
                    if (field.Field.Name == _constants.CountKeyAlias)
                    { 
                        HandleSelectCountColumn(request, field);
                    }

                    result.HavingColumnValue(aggregate, field.TableAlias, field.Field.Name, compareOperator, filter.Values[0], filter.Column.DbType, filter.Column.UniqueName, isNullValue);
                }
                else
                {
                    var collection = new HavingFilterCollection();
                    foreach (var v in filter.Values)
                    {

                        // theres a special case for handling counts, if the referenced table is not a CTE, then we should actually select 1
                        if (field.Field.Name == _constants.CountKeyAlias)
                        {
                            HandleSelectCountColumn(request, field);
                        }

                        collection.HavingColumnValue(aggregate, field.TableAlias, field.Field.Name, compareOperator, v, filter.Column.DbType, filter.Column.UniqueName);
                    }
                    result.HavingCollection(Combine.Or, collection);
                }

            }

            return result;
        }

        protected virtual void HandleSelectCountColumn(MappedSearchRequest request, Column field)
        {
            var columnTable = _tableMappings.GetAllTables().FirstOrDefault(x => x.Alias == field.TableAlias);

            if (columnTable == null
                || columnTable.KnownTableName == request.GroupByColumn.KnownTable
                || !GroupByIsChildTable(field.TableAlias, request.GroupByColumn)
                || columnTable.TableType == TableType.Stats)
            {
                field.TableAlias = null;
                field.Field.Name = "1";

                // when there are multiple stats in a date range, inherit the count from the date stats table
                if (columnTable != null && columnTable.TableType == TableType.Stats && IsDateQuery(request))
                {
                    field.TableAlias = "Stats";
                    field.Field.Name = "_C";
                }
            }
        }

        // Restrict
        protected virtual TemporalAggregation RestrictResolution(MappedSearchRequest request)
        {
            return request.TemporalAggregation;
        }

        protected virtual ReportColumnMapping RestrictSort(MappedSearchRequest request, bool forOrderBy = false)
        {
            return request.SortByColumn;
        }

        protected virtual ReportColumnMapping RestrictGroupBy(MappedSearchRequest request)
        {
            return request.GroupByColumn;
        }

        protected virtual List<ReportColumnMapping> RestrictColumns(MappedSearchRequest request)
        {
            return request.SelectedColumns;
        }

        protected virtual List<MappedSearchRequestFilter> RestrictFilters(MappedSearchRequest request, bool isWhere)
        {
            return request.Filters;
        }
        
        // Override
        protected virtual bool IsDateQuery(MappedSearchRequest request)
        {
            return request.DateStart.HasValue || request.TemporalAggregation != TemporalAggregation.Total;
            /* The request is a for specific date range (i.e. not a lifetime stats request). So we'll have to query the daily and / or hourly stats tables. */
            /*  OR We've been asked to return stats data broken down by day or hour so we'll have to query the daily and / or hourly stats tables. */
            
        }

        protected virtual bool CanSelectRowNumber()
        {
            return true;
        }

        /// <summary>
        /// Determines whether the TextFilter Text where filter should be applied for this query
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanFilterOnText()
        {
            return true;
        }
        
        /// <summary>
        /// Gets the Column with alias for non calculated columns
        /// Gets the Column without alias for calculated columns and handles the conversion of calculated columns to sql
        /// For calculated colums, the column will be expanded to a full formula
        /// Sets the result on the column mapping so that it doesnt need to be re-calculated - this works with the cache
        /// </summary>  
        public virtual Column GetColumnSelector(ReportColumnMapping col, MappedSearchRequest request, bool dontAggregate = false, bool useFieldAlias = false)
        { 
            var cached = col.CalculatedValues.GetColumnTableAndField(dontAggregate, useFieldAlias);
            if (cached != null)
            {
                return new Column(cached[0], cached[1]);
            } 

            var result = GetColumSelectorInternal(col, dontAggregate, useFieldAlias);

            col.CalculatedValues.SetColumnTableAndField(dontAggregate, useFieldAlias, result.TableAlias, result.Field.Name);

            return result; 
        }

        private Column GetColumSelectorInternal(ReportColumnMapping col, bool dontAggregate = false, bool useFieldAlias = false)
        {
            Column result;

            using (new DebugTimer("SearchQueryBuilder.GetColumSelectorInternal"))
            {
                var table = col.KnownTable; 

                string tableAlias = QueryHelpers.GetTableAlias(table);
                  
                if (col.ActionSpecId > 0 && (QueryHelpers).IsTransposeStatColumn(col))
                {
                    tableAlias += "_" + col.ActionSpecId;
                } 
                 
                string fieldName = GetReplacedFieldName(col, table);

                if (Regex.IsMatch(col.FieldName, @"^[0-9.]+$"))
                {
                    tableAlias = null;
                }

                // expand and replace fields in calculated columns
                if ((col.IsCalculated || MagiQL.DataAdapters.Infrastructure.Sql.QueryHelpers.IsCalculatedColumn(col.FieldName)) && col.FieldName != "_C")
                {
                    // for perforance, only calculate it if its not been done before, and store it on the object
                    var fieldNameCached = col.CalculatedValues.GetCalculatedColFieldName(dontAggregate, useFieldAlias);
                    if (fieldNameCached == null)
                    {
                        fieldNameCached = _calculatedColumnHelper.GetCalculatedColumnFieldName(fieldName, false, dontAggregate, useFieldAlias);
                        col.CalculatedValues.SetCalculatedColFieldName(dontAggregate, useFieldAlias, fieldNameCached);
                    }
                    fieldName = fieldNameCached;

                    result = new Column(null, fieldName);
                }
                else
                {
                    result = new Column(tableAlias, fieldName);
                }
            }
            return result;
        }
        
        protected virtual string GetFieldAlias(SelectedColumn column)
        {
            return QueryHelpers.GetFieldAlias(column);
        }

        protected virtual string GetFieldAlias(ReportColumnMapping column, MappedSearchRequest request)
        {
            return QueryHelpers.GetFieldAlias(column);
        }
         
        protected string GetTableName(string table)
        {
            return _tableMappings.GetDataTableName(table);
        }
         
        protected string GetStatsTableName(
            string table, string joinTable, 
            TemporalAggregation temporalAggregation,
            DateRangeType dateRangeType,
            DateTime? startDate,
            DateTime? endDate)
        {
            return _tableMappings.GetStatsTableName(table, joinTable, temporalAggregation, dateRangeType, startDate, endDate);
        }

        protected virtual List<ReportColumnMapping> RemoveDateColumnsIfNonDateQuery(MappedSearchRequest request, List<ReportColumnMapping> result)
        {
            if (!IsDateQuery(request))
            {
                result = result.Where(x => !IsDateColumn(x)).ToList();
            }
            return result;
        }

        public virtual bool IsDateColumn(ReportColumnMapping x)
        {
            return x.Id == _constants.DateStatColumnId || x.Id == _constants.HourStatColumnId;
        }
 
        public abstract bool RequireCurrencyGroupBy(MappedSearchRequest request);
    
        /// <summary>
        /// for calculated columns, replace [] table names with actual names
        /// </summary> 
        /// <returns></returns>
        public virtual string GetReplacedFieldName(string fieldName, string mainTableAlias)
        {
            fieldName = fieldName.Replace("[0]", "[" + mainTableAlias + "]");

            return fieldName;
        }

        /// <summary>
        /// for calculated columns, replace [] table names with actual names
        /// </summary> 
        /// <returns></returns>
        public string GetReplacedFieldName(ReportColumnMapping col, string mainTable)
        {
            var fieldName = col.FieldName;
            string mainTableAlias = QueryHelpers.GetTableAlias(mainTable);
            return GetReplacedFieldName(fieldName, mainTableAlias);
        }
        
        protected bool CanJoinTables(string newKnownTable, string existingKnownTable)
        {
            var relationships = GetTableRelationships(newKnownTable, existingKnownTable);

            return relationships.Count() == 1;
        }

        protected void JoinTables(SelectQuery query, string newKnownTable, string existingKnownTable, string newTableNameOverride = null, string existingTableNameOverride = null)
        {
            var relationships = GetTableRelationships(newKnownTable, existingKnownTable);

            if (relationships.Count() != 1)
            {
                throw new Exception(string.Format("Could not join {0} to {1}", newKnownTable, existingKnownTable));
            }

            var relationship = relationships.First();

            if (relationship.Table1.KnownTableName == newKnownTable)
            {
                query.LeftJoin(newTableNameOverride ?? relationship.Table1.DbTableName, relationship.Table1.Alias, relationship.Table1Column,
                    existingTableNameOverride ?? relationship.Table2.Alias, relationship.Table2Column);
            }
            else
            {
                query.LeftJoin(newTableNameOverride ?? relationship.Table2.DbTableName, relationship.Table2.Alias, relationship.Table2Column,
                               existingTableNameOverride ?? relationship.Table1.Alias, relationship.Table1Column);
            }
        }

        protected IEnumerable<TableRelationship> GetTableRelationships(
            string newKnownTable,
            string existingKnownTable)
        {
            var relationships = _tableMappings.GetAllTableRelationships().Where(x =>
                (x.Table1.KnownTableName == newKnownTable && x.Table2.KnownTableName == existingKnownTable)
                || (x.Table2.KnownTableName == newKnownTable && x.Table1.KnownTableName == existingKnownTable)
                );
            return relationships;
        }

        public virtual bool TableIsChildOfTable(string childCandidateTableName, string parentCandidateTableName)
        {
            if (childCandidateTableName == parentCandidateTableName)
            {
                return false;
            }

            // warning, this is not currently recursive but may be needed (with a flag param)

            var parentRelationships = _tableMappings.GetAllTableRelationships()
                .Where(x => x.Table1.KnownTableName == parentCandidateTableName
                         || x.Table2.KnownTableName == parentCandidateTableName);

            foreach (var rel in parentRelationships)
            {
                if (rel.RelationshipType == TableRelationshipType.OneToMany
                    && rel.Table1.KnownTableName == parentCandidateTableName
                    && rel.Table2.KnownTableName == childCandidateTableName)
                {
                    return true;
                }
                if (rel.RelationshipType == TableRelationshipType.ManyToOne
                    && rel.Table1.KnownTableName == childCandidateTableName
                    && rel.Table2.KnownTableName == parentCandidateTableName)
                {
                    return true;
                }
            }


            return false;
        }

        public virtual bool TableIsSiblingOfTable(string leftCandidateTableName, string rightCandidateTableName)
        {
            if (leftCandidateTableName == rightCandidateTableName)
            {
                return false;
            }

            var siblingRelationships = _tableMappings.GetAllTableRelationships()
                .Where(x => x.RelationshipType == TableRelationshipType.OneToOne
                        && (x.Table1.KnownTableName == rightCandidateTableName || x.Table2.KnownTableName == rightCandidateTableName)
                        );

            foreach (var rel in siblingRelationships)
            {
                if (rel.Table1.KnownTableName == leftCandidateTableName
                    || rel.Table2.KnownTableName == leftCandidateTableName)
                {
                    return true;
                }
            }

            return false;
        }
        
        public virtual bool TableIsParentOfTable(string parentCandidateTableName, string childCandidateTableName)
        {
            return TableIsChildOfTable(childCandidateTableName, parentCandidateTableName);
        }

        public virtual bool ColumnIsTableOrForeignKeyToTable(
            ReportColumnMapping column,
            TableMapping joinTable)
        {
            if (column.KnownTable == joinTable.KnownTableName)
            {
                return true;
            }

            // not sure if this will be needed. dont delete yet
            //            // if check if the foreign keys match
            //            var relationships = GetTableRelationships(column.KnownTable , joinTable.KnownTableName).ToList();
            //
            //            foreach (var r in relationships)
            //            {
            //                if (r.Table1.KnownTableName == joinTable.KnownTableName 
            //                 && r.Table2.KnownTableName == column.KnownTable)
            //                {
            //                    if (r.Table1Column == joinTable.PrimaryKey 
            //                     && r.Table2Column == column.FieldName)
            //                    {
            //                        return true;
            //                    }
            //                }
            //                else if (r.Table2.KnownTableName == joinTable.KnownTableName
            //                      && r.Table1.KnownTableName == column.KnownTable)
            //                {
            //                    if (r.Table2Column == joinTable.PrimaryKey
            //                    &&  r.Table1Column == column.FieldName)
            //                    {
            //                        return true;
            //                    }
            //                }
            //            }

            return false;
        }

    }
}