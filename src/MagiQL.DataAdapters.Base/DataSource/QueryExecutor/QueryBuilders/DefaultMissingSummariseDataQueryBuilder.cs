using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Framework.Model.Columns;
using SqlModeller.Interfaces;
using SqlModeller.Model;
using SqlModeller.Model.Select;
using SqlModeller.Shorthand;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders
{
    // when summarizing, if the query should include results which are not available from the Root table
    // then select missing rows from the summarize by table to be unioned with the data cte
    // if the summarise table cannot support the filters for the query, then these rows should not show, so there would no need to generate this cte
    public class DefaultMissingSummariseDataQueryBuilder : DefaultDataQueryBuilder
    {
        public DefaultMissingSummariseDataQueryBuilder(IDataSourceComponents dataSourceComponents) : base(dataSourceComponents)
        {
        }

        public virtual bool IncludeInQuery(MappedSearchRequest request)
        {
            return IsRequired(request) && RequestIsSupported(request);
        }

        public virtual bool IsRequired(MappedSearchRequest request)
        {
            return false; // this component must be manually enabled in the implementation by overriding the result
        }

        public virtual bool RequestIsSupported(MappedSearchRequest request)
        {
            return FiltersAreSupported(request) && QueryFilterIsSupported(request);
        }

        public virtual bool FiltersAreSupported(MappedSearchRequest request)
        {
            if (request.Filters != null)
            { 
                foreach (var filter in request.Filters)
                {
                    if (filter.Column.KnownTable != request.SummarizeByColumn.KnownTable)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual bool QueryFilterIsSupported(MappedSearchRequest request)
        {
            if (string.IsNullOrEmpty(request.TextFilter))
            {
                return true;
            }

            // we can only support the text search if its searching the table we are summarizing by

            var queryColumns = request.TextFilterColumns;
            if ((queryColumns == null || !queryColumns.Any()) && _constants.TextSearchColumnId > 0)
            {
                // set queryColumns to the default column
                var defaultTextSearchColumn = _columnProvider.GetColumnMapping(1, _constants.TextSearchColumnId);
                queryColumns = new List<ReportColumnMapping>() { defaultTextSearchColumn };
            }

            foreach (var column in queryColumns)
            {
                if (column.KnownTable != request.SummarizeByColumn.KnownTable)
                {
                    return false;
                }
            }

            return true;
        }


        public override void BuildSelect(
            SelectQuery query,
            List<ReportColumnMapping> selectedColumns,
            ReportColumnMapping sortColumn,
            ReportColumnMapping groupByColumn,
            MappedSearchRequest request)
        { 
            var summarizeTableAlias = _tableMappings.GetTableMapping(request.SummarizeByColumn.KnownTable).Alias;

            _dataSourceComponents.DataQueryBuilder.BuildSelect(query, selectedColumns, sortColumn, groupByColumn, request);
            // build the select query that would be built by the data query builder 
            // and then replace the select columns with null values if not from the summarize table

            var newSelectColumns = new List<IColumnSelector>(); 

            foreach (var selectColumn in query.SelectColumns)
            {
                if (selectColumn is ColumnDatePartSelector)
                {
                    var c = selectColumn as ColumnDatePartSelector;
                    newSelectColumns.Add(new SqlColumnSelector("NULL AS " + c.Alias));
                }

                else if (selectColumn is ColumnSelector)
                {
                    var c = selectColumn as ColumnSelector;
                    if (c.TableAlias != summarizeTableAlias)
                    {
                        newSelectColumns.Add(new SqlColumnSelector("NULL AS " + c.Alias));
                    }
                    else
                    {
                        newSelectColumns.Add(selectColumn);
                    }
                }

                else if (selectColumn is CountColumnSelector)
                {
                    var c = selectColumn as CountColumnSelector; 
                    newSelectColumns.Add(new SqlColumnSelector("NULL AS " + c.Alias));
                }

                else if (selectColumn is GroupByColumnSelector)
                {
                    var c = selectColumn as GroupByColumnSelector; 
                    newSelectColumns.Add(new SqlColumnSelector("NULL AS " + c.Alias));
                }

                else if (selectColumn is OrderByColumnSelector)
                {
                    var c = selectColumn as OrderByColumnSelector;
                    if (request.SortByColumn.KnownTable != request.SummarizeByColumn.KnownTable)
                    {
                        newSelectColumns.Add(new SqlColumnSelector("NULL AS " + c.Alias));
                    }
                    else
                    {
                        newSelectColumns.Add(selectColumn);
                    }
                }

                else if (
                    selectColumn is RowNumberColumnSelector
                    || selectColumn is TotalColumnSelector 
                    )
                {
                    newSelectColumns.Add(selectColumn);
                    continue;
                }

                else
                {
                    throw new NotSupportedException("Cannot nullify columns of type " + selectColumn.GetType().FullName);
                }
            }

            query.SelectColumns = newSelectColumns;

        }

        public override void BuildFrom(
            SelectQuery query,
            List<ReportColumnMapping> selectedColumns,
            ReportColumnMapping groupByColumn,
            ReportColumnMapping sortByColumn,
            MappedSearchRequest request)
        {
            var fromTableName = request.SummarizeByColumn.KnownTable;
            var fromTable = _tableMappings.GetTableMapping(fromTableName);

            query.From(fromTable.DbTableName, fromTable.Alias);
        }

        protected override void BuildGroupBy(
            SelectQuery query,
            ReportColumnMapping groupByColumn,
            MappedSearchRequest request)
        {
            base.BuildGroupBy(query, request.SummarizeByColumn, request);
        } 
    }
}
