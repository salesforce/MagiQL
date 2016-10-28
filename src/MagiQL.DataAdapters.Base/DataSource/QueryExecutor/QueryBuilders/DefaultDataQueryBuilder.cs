using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;
using SqlModeller.Model;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders
{
    /// <summary>
    /// SELECT :
    ///  - _GroupKey
    ///  - _C = Count 
    ///  - _CurrencyKey = currencycode 
    ///  - If Order By is not a Stat (data table) : select _R (row number, for sorting later)
    ///  - Non Stats columns in requested columns (data table) 
    /// 
    /// COLUMN NAMING : 
    ///  -  ALL columns use a generated alias
    /// 
    /// FROM : 
    ///  - root data table
    ///  - JOIN : other tables required for selecting, sorting, filtering (dependant)
    ///  
    /// GROUP BY : 
    ///  - Request.GroupBy column
    /// 
    /// WHERE : 
    ///  - All filters in Request which are not aggregatable 
    /// 
    /// HAVING : 
    ///  - Filters in request which are based on aggregated values
    /// 
    /// ORDER BY : 
    ///  - Used to get the row number if ordering by a data value
    /// 
    /// </summary>
    public class DefaultDataQueryBuilder : QueryBuilderBase
    {
        protected readonly IDataSourceComponents _dataSourceComponents;

        public DefaultDataQueryBuilder(IDataSourceComponents dataSourceComponents) : base(dataSourceComponents)
        {
            _dataSourceComponents = dataSourceComponents;
        }
          
        // Restrict
        protected override List<ReportColumnMapping> RestrictColumns(MappedSearchRequest request)
        {
            var result = request.SelectedColumns.ToList();
            result.AddRange(request.DependantColumns);
            
            // remove stats columns
            result = result.Where(x => !QueryHelpers.IsStatsColumn(x)).ToList(); 
            return result;
        }

        protected override List<MappedSearchRequestFilter> RestrictFilters(MappedSearchRequest request, bool isWhere)
        {
            if (request.Filters == null || !request.Filters.Any())
            {
                return request.Filters;
            }

            // remove stats columns
            var result = request.Filters.Where(x => !QueryHelpers.IsStatsColumn(x.Column));

            // remove aggregated columns for where filters and non aggregated columns for having filters
            result = result.Where(x => x.ProcessBeforeAggregation == isWhere);

            return result.ToList();
        }
        
        protected override TemporalAggregation RestrictResolution(MappedSearchRequest request)
        {
            return TemporalAggregation.Total;
        }

        protected override ReportColumnMapping RestrictSort(MappedSearchRequest request, bool forOrderBy = false)
        {
            if (request.SortByColumn != null)
            {
                // remove stats columns
                return QueryHelpers.IsStatsColumn(request.SortByColumn) ? null : request.SortByColumn;
            }
            return null;
        }

        protected override ReportColumnMapping RestrictGroupBy(MappedSearchRequest request)
        {
            if (QueryHelpers.IsStatsColumn(request.GroupByColumn))
            {
                // we cannot group by a stats column, so we need to group by the column stats join onto
                
                // get the stats relationship to the data join table
                var statsRelationship = _tableMappings.GetAllTableRelationships().FirstOrDefault(x => x.Table1 is StatsTableMapping || x.Table2 is StatsTableMapping);
                
                if (statsRelationship != null)
                {
                    var dataTable = statsRelationship.Table1.TableType == TableType.Data
                        ? statsRelationship.Table1
                        : statsRelationship.Table2;

                    var primaryKeyColumn = _columnProvider.Find(_constants.DataSourceId, dataTable.KnownTableName, dataTable.PrimaryKey, null);
                    if (primaryKeyColumn != null && primaryKeyColumn.Count==1)
                    {
                        return primaryKeyColumn.First();
                    }
                }
            }
            return base.RestrictGroupBy(request);
        }

        // Override
        public override ReportColumnMapping GetCurrencyColumn()
        {
            return _columnProvider.GetColumnMapping(_constants.DataSourceId, _constants.CurrencyColumnId);
        }
        
        public override bool IsDateColumn(ReportColumnMapping x)
        {
            return x.Id == _constants.DateStatColumnId || x.Id == _constants.HourStatColumnId;
        }
        
        // todo : move this somewhere else
        public override bool RequireCurrencyGroupBy(MappedSearchRequest request)
        {
            return _dataSourceComponents.QueryBuilderBase.RequireCurrencyGroupBy(request);
        }

        public override bool GroupByIsChildTable(string tableAlias, ReportColumnMapping groupByColumn)
        {
            return  _dataSourceComponents.QueryBuilderBase.GroupByIsChildTable(tableAlias, groupByColumn);
        }

        public override void BuildFrom(SelectQuery query, List<ReportColumnMapping> selectedColumns, ReportColumnMapping groupByColumn, ReportColumnMapping sortByColumn, MappedSearchRequest request)
        {
            _dataSourceComponents.QueryBuilderBase.BuildFrom(query, selectedColumns, groupByColumn, sortByColumn, request);
        }
         
        public override bool WhereFilterColumnAllowIsNull(MappedSearchRequestFilter filter)
        {
            return _dataSourceComponents.QueryBuilderBase.WhereFilterColumnAllowIsNull(filter);
        }
    }
}