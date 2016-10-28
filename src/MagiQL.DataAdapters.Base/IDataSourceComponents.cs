using System;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Validation;
using MagiQL.Framework.Interfaces;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Stats;
using MagiQL.Reports.DataAdapters.Base.Mappers;

namespace MagiQL.Reports.DataAdapters.Base
{
    public interface IDataSourceComponents
    {
        IColumnProvider ColumnProvider { get; }
        IReportColumnMappingValidator ColumnMappingValidator { get; set; }

        DefaultQueryHelpers QueryHelpers { get; }
        DefaultCalculatedColumnHelper CalculatedColumnHelper { get; }
        DefaultSearchRequestMapper SearchRequestMapper { get; }
        ConstantsBase Constants { get; }
        TableMappingsBase TableMappings { get; }

        DefaultSearchQueryBuilder SearchQueryBuilder { get;}
        DefaultDateStatsCteQueryBuilder DateStatsQueryBuilder { get; set; }
        DefaultTransposeStatsCteQueryBuilder TransposeStatsQueryBuilder { get; set; }
        QueryBuilderBase QueryBuilderBase { get; set; }
        DefaultStatsQueryBuilder StatsQueryBuilder { get; set; }
        DefaultDataQueryBuilder DataQueryBuilder { get; set; }
        DefaultOneToManyCteQueryBuilderFactory OneToManyCteQueryBuliderFactory { get; set; }
        DefaultMissingSummariseDataQueryBuilder MissingSummarizeDataQueryBuilder { get; set; }

    }

    public abstract class DataSourceComponentsBase : IDataSourceComponents
    {
        // injected
        public IColumnProvider ColumnProvider { get; protected set; }
        // implemented
        public ConstantsBase Constants { get; protected set; }
        public TableMappingsBase TableMappings { get; protected set; }
        // derived
        public DefaultQueryHelpers QueryHelpers { get; protected set; }
        public DefaultCalculatedColumnHelper CalculatedColumnHelper { get; protected set; }
        public DefaultSearchRequestMapper SearchRequestMapper { get; protected set; }

        // query builders
        public DefaultSearchQueryBuilder SearchQueryBuilder { get; protected set; }
        public DefaultDateStatsCteQueryBuilder DateStatsQueryBuilder { get; set; }
        public DefaultTransposeStatsCteQueryBuilder TransposeStatsQueryBuilder { get; set; }
        public QueryBuilderBase QueryBuilderBase { get; set; }
        public DefaultStatsQueryBuilder StatsQueryBuilder { get; set; }
        public DefaultDataQueryBuilder DataQueryBuilder { get; set; }
        public DefaultOneToManyCteQueryBuilderFactory OneToManyCteQueryBuliderFactory { get; set; }
        public DefaultMissingSummariseDataQueryBuilder MissingSummarizeDataQueryBuilder { get; set; }
        public IReportColumnMappingValidator ColumnMappingValidator { get; set; }


        protected DataSourceComponentsBase(IColumnProvider columnProvider)
        {
            // injected
            ColumnProvider = columnProvider;
            
            // implemented
            RegisterComponents();

            // populate constants
            if (Constants == null) { throw new Exception("Constants is not registered"); }
            PopulateColumnsInConstants(Constants, ColumnProvider);

            // set defaults for missing properties
            SearchRequestMapper = SearchRequestMapper ?? new DefaultSearchRequestMapper(this);
            QueryHelpers = QueryHelpers ?? new DefaultQueryHelpers(ColumnProvider, Constants, TableMappings); 
            CalculatedColumnHelper = CalculatedColumnHelper ?? new DefaultCalculatedColumnHelper(ColumnProvider, TableMappings, Constants, QueryHelpers);

            TransposeStatsQueryBuilder = TransposeStatsQueryBuilder ?? new DefaultTransposeStatsCteQueryBuilder(this);
            DateStatsQueryBuilder = DateStatsQueryBuilder ?? new DefaultDateStatsCteQueryBuilder(this);
            StatsQueryBuilder = StatsQueryBuilder ?? new DefaultStatsQueryBuilder(this);
            DataQueryBuilder = DataQueryBuilder ?? new DefaultDataQueryBuilder(this);
            OneToManyCteQueryBuliderFactory = OneToManyCteQueryBuliderFactory ?? new DefaultOneToManyCteQueryBuilderFactory(this);
            MissingSummarizeDataQueryBuilder = MissingSummarizeDataQueryBuilder ?? new DefaultMissingSummariseDataQueryBuilder(this);
            SearchQueryBuilder = SearchQueryBuilder ?? new DefaultSearchQueryBuilder(this);
            ColumnMappingValidator = ColumnMappingValidator ?? new DefaultColumnMappingValidator();
              
            if (TableMappings == null) { throw new Exception("TableMappings is not registered"); }
            if (QueryHelpers == null) { throw new Exception("QueryHelpers is not registered"); }
            if (CalculatedColumnHelper == null) { throw new Exception("CalculatedColumnHelper is not registered"); }
            if (SearchRequestMapper == null) { throw new Exception("SearchRequestMapper is not registered"); }

            if (SearchQueryBuilder == null) { throw new Exception("SearchQueryBuilder is not registered"); }
            if (DateStatsQueryBuilder == null) { throw new Exception("DateStatsQueryBuilder is not registered"); }
            if (TransposeStatsQueryBuilder == null) { throw new Exception("TransposeStatsQueryBuilder is not registered"); }
            if (QueryBuilderBase == null) { throw new Exception("QueryBuilderBase is not registered"); }
            if (StatsQueryBuilder == null) { throw new Exception("StatsQueryBuilder is not registered"); }
            if (DataQueryBuilder == null) { throw new Exception("DataQueryBuilder is not registered"); }
            if (OneToManyCteQueryBuliderFactory == null) { throw new Exception("OneToManyCteQueryBuliderFactory is not registered"); }
            if (MissingSummarizeDataQueryBuilder == null) { throw new Exception("MissingSummarizeDataQueryBuilder is not registered"); }

            ColumnProvider.Initialize(); 

        }

        private void PopulateColumnsInConstants(ConstantsBase constants, IColumnProvider columnProvider)
        {
            try
            {
                constants.CurrencyColumnId = GetColumnId(constants.CurrencyColumnUniqueName, constants, columnProvider);
                constants.DateStatColumnId = GetColumnId(constants.DateStatColumnUniqueName, constants, columnProvider);
                constants.HourStatColumnId = GetColumnId(constants.HourStatColumnUniqueName, constants, columnProvider);
                constants.TextSearchColumnId = GetColumnId(constants.TextSearchColumnUniqueName, constants, columnProvider);
            }
            catch (Exception ex)
            {
                // todo : store the exception for later
            }
        }

        private static int GetColumnId(string uniqueName, ConstantsBase constants, IColumnProvider columnProvider)
        {
            if (!string.IsNullOrEmpty(uniqueName))
            {
                var columns = columnProvider.Find(constants.DataSourceId, uniqueName, cacheOnly: false);
                
                if (!columns.Any())
                {
                    throw new Exception(
                        string.Format(
                            "Error initializing Constants for DataSource Id:{0} ({1}): No column found with UniqueName '{2}'",
                            constants.DataSourceId, constants.Platform, uniqueName));
                }
                
                if (columns.Count() > 1)
                {
                    throw new Exception(
                        string.Format(
                            "Error initializing Constants for DataSource Id:{0} ({1}): More than 1 column found with UniqueName '{2}'",
                            constants.DataSourceId, constants.Platform, uniqueName));
                }

                return columns.Single().Id;
            }
            return -1;
        }

        protected abstract void RegisterComponents();

        
    }
}