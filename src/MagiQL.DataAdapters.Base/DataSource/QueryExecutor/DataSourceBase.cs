using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;
using MagiQL.Reports.DataAdapters.Base.Mappers;
using SqlModeller.Model;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor
{
    public abstract class DataSourceBase : IReportsDataSource
    {
        private readonly IDataSourceComponents _dataSourceComponents;

        public virtual string Platform { get { return _constants.Platform; } }
        public virtual int DataSourceId { get { return _constants.DataSourceId; } }
        public virtual string ConnectionStringName { get { return _constants.ConnectionStringName; } }

        protected readonly IColumnProvider columnProvider;
        protected readonly DefaultQueryHelpers QueryHelpers;
        protected readonly DefaultCalculatedColumnHelper _calculatedColumnHelper;
        protected readonly DefaultSearchRequestMapper requestMapper;
        protected readonly ConstantsBase _constants;
        protected readonly TableMappingsBase _tableMappings;
        private IReportColumnMappingValidator _columnValidator;

        protected DataSourceBase(IDataSourceComponents dataSourceComponents)
        {
            _dataSourceComponents = dataSourceComponents;
            columnProvider = dataSourceComponents.ColumnProvider;
            QueryHelpers = dataSourceComponents.QueryHelpers;
            _calculatedColumnHelper = dataSourceComponents.CalculatedColumnHelper;
            requestMapper = dataSourceComponents.SearchRequestMapper;
            _constants = dataSourceComponents.Constants;
            _tableMappings = dataSourceComponents.TableMappings;
            _columnValidator = dataSourceComponents.ColumnMappingValidator;
        }

        public List<ReportColumnMapping> GetDependantColumnMappings(int dataSourceId,int columnId)
        {
            var columnMapping = _dataSourceComponents.ColumnProvider.GetColumnMapping(dataSourceId, columnId);
            if (!columnMapping.IsCalculated)
            {
                return null;
            }
            var foundColumns = _calculatedColumnHelper.FindColumnsInCalculatedField(columnMapping.FieldName);
            return foundColumns.Select(c => c.Key).ToList();
        }
        
        public List<ReportColumnMapping> GetDependantColumnMappings(int dataSourceId,string fieldName)
        {
            var foundColumns = _calculatedColumnHelper.FindColumnsInCalculatedField(fieldName);
            return foundColumns.Select(c => c.Key).ToList();
        }
          
        public virtual IColumnProvider GetColumnProvider()
        {
            return columnProvider;
        }

        public IReportColumnMappingValidator GetColumnValidator()
        {
            return _columnValidator;
        }

        public virtual Query BuildQuery(
            SearchRequest request,
            out long mapTime)
        {
            MappedSearchRequest mappedRequest;
            long mapperTime = 0;
            using (new QuickTimer(x => mapperTime = x))
            {
                mappedRequest = requestMapper.Map(request);
            }

            InitializeFilters(mappedRequest.Filters);

            mapTime = mapperTime;
            var queryBuilder = GetQueryBuilder();
            return queryBuilder.Build(mappedRequest);
        }
         
        protected QueryBuilderBase GetQueryBuilder()
        {
            return _dataSourceComponents.SearchQueryBuilder;
        }

        public virtual List<ColumnDefinition> GetAllSelectableColumnDefinitions(
            int? organizationId,
            int? groupBy = null)
        {
            return columnProvider.GetAllSelectableColumnDefinitions(DataSourceId, organizationId, groupBy);
        }
        
        public virtual List<ReportColumnMapping> GetColumnMappings(List<SelectedColumn> selectedColumns)
        {
            var allIds = selectedColumns.Select(x => x.ColumnId).ToList();
            return columnProvider.GetColumnMappings(DataSourceId, allIds).ToList();
        }

        public virtual string GetColumnDisplayName(ColumnDefinition col)
        {
            return QueryHelpers.GetDisplayName(col);
        }

        public virtual string GetFieldAlias(ReportColumnMapping col)
        {
            return QueryHelpers.GetFieldAlias(col);
        }
        
        public TableInfo GetTableInfo()
        {
            var relations = _tableMappings.GetAllTableRelationships();
            var graphBuilder = new TableRelationshipGraphBuilder();
            var graph = graphBuilder.Build(relations, _constants.RootTableName);
            var tables = _tableMappings.GetAllTables();
            tables = tables.OrderBy(x => graphBuilder.GetDistance(graph, x.KnownTableName)).ToList();

            return new TableInfo
            {
                Tables = tables,
                Relationships = relations
            };
        }

        public object GetConfiguration()
        {
            var tableInfo = GetTableInfo();
            return new
            {
                Constants = _constants,
                TableInfo = tableInfo
            };
        }

        /// <summary>
        /// Sets ProcessBeforeAggregation on all filters based on aggregation method
        /// </summary>
        /// <param name="filters"></param>
        protected void InitializeFilters(List<MappedSearchRequestFilter> filters)
        {
            // this is used in the RestrictFilter methods for data and stats CTEs. 
            // If a column is using a First aggregation method, such as UI Status, we know not to filter on the aggregate value.

            if (filters == null || !filters.Any())
            {
                return;
            }

            foreach (var f in filters)
            {
                f.ProcessBeforeAggregation = !IsAggregated(f);
            }
        }

        protected internal bool IsAggregated(MappedSearchRequestFilter filter)
        {
            var aggregationMethod = filter.Column.FieldAggregationMethod;
            return (aggregationMethod != FieldAggregationMethod.First &&
                    aggregationMethod != FieldAggregationMethod.Exclude);
        }

         
    }
}
