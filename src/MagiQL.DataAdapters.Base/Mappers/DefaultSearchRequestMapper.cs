using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using MagiQL.Reports.DataAdapters.Base.DataSource.ColumnMappings;
using SqlModeller.Model;

namespace MagiQL.Reports.DataAdapters.Base.Mappers
{
    public class DefaultSearchRequestMapper : SearchRequestMapperBase
    {
        protected readonly IDataSourceComponents _dataSourceComponents;
        protected DefaultQueryHelpers QueryHelpers { get { return _dataSourceComponents.QueryHelpers; } }
        protected DefaultCalculatedColumnHelper _calculatedColumnHelper { get { return _dataSourceComponents.CalculatedColumnHelper; } }
        protected IColumnProvider _columnProvider { get { return _dataSourceComponents.ColumnProvider; } }
        protected ConstantsBase _constants { get { return _dataSourceComponents.Constants; } }
        protected TableMappingsBase _tableMappings { get { return _dataSourceComponents.TableMappings; } }

        public DefaultSearchRequestMapper(IDataSourceComponents dataSourceComponents)
        {
            _dataSourceComponents = dataSourceComponents; 
        }

        public override ReportColumnMapping GetColumnMapping(SelectedColumn column)
        {
            if (column == null)
            {
                return null;
            }
            return _columnProvider.GetColumnMapping(_constants.DataSourceId, column.ColumnId);
        }
        
        public override List<ReportColumnMapping> GetDependantColumnMappings(MappedSearchRequest request)
        { 
            // should come from select, group, sort and calculated columns

            var allColumns = request.SelectedColumns.ToList();
            var notSelectedColumns = new List<ReportColumnMapping>();

            // Add Group By Column
            if (request.GroupByColumn != null && allColumns.All(x => x.Id != request.GroupByColumn.Id))
            {
                allColumns.Add(request.GroupByColumn);
                notSelectedColumns.Add(request.GroupByColumn);
            }

            // Add Summarize By Column
            if (request.SummarizeByColumn != null && allColumns.All(x => x.Id != request.SummarizeByColumn.Id))
            {
                allColumns.Add(request.SummarizeByColumn);
                notSelectedColumns.Add(request.SummarizeByColumn);
            }

            // Add Sort By Column
            if (request.SortByColumn != null && allColumns.All(x => x.Id != request.SortByColumn.Id))
            {
                allColumns.Add(request.SortByColumn);
                notSelectedColumns.Add(request.SortByColumn);
            }

            // Add Filter Columns
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var f in request.Filters)
                {
                    if (f.Column != null && allColumns.All(x => x.Id != f.Column.Id))
                    {
                        allColumns.Add(f.Column);
                        notSelectedColumns.Add(f.Column);
                    }
                }
            }

            // Add Text Filter Columns
            if (request.TextFilterColumns != null && request.TextFilterColumns.Any())
            {
                foreach (var f in request.TextFilterColumns)
                {
                    if (f != null && allColumns.All(x => x.Id != f.Id))
                    {
                        allColumns.Add(f);
                        notSelectedColumns.Add(f);
                    }
                }
            }

            // Add Columns Used By Calculations
            var allCalculatedColumns = allColumns.Where(x => QueryHelpers.IsCalculatedColumn(x)).ToList();

            bool isSummarizing = request.SummarizeByColumn != null;
            var nestedCalculatedColumns = GetAllColumnsUsedByCalculatedColumns(allCalculatedColumns, isSummarizing);
            
            foreach (var col in nestedCalculatedColumns)
            {
                if (allColumns.All(x => x.Id != col.Id))
                {
                    allColumns.Add(col);
                    notSelectedColumns.Add(col);
                }
            }


            // Add TransposeStats Columns
            var transposeStatColumns = FindTransposeStatsColumnsInCalculation(allColumns);
            foreach (var col in transposeStatColumns)
            {
                if (allColumns.All(x => x.Id != col.Id))
                {
                    allColumns.Add(col);
                    notSelectedColumns.Add(col);
                }
            } 

            AddAdditionalDependantColumnMappings(request, allColumns, notSelectedColumns); 

            return notSelectedColumns;

        }

        protected virtual void AddAdditionalDependantColumnMappings(MappedSearchRequest request,
            List<ReportColumnMapping> allColumns,
            List<ReportColumnMapping> notSelectedColumns)
        {
            
        }
        
        public List<ReportColumnMapping> FindTransposeStatsColumnsInCalculation(IEnumerable<ReportColumnMapping> selectedColumns)
        {
            var result = new List<ReportColumnMapping>();

            var transposeTable = _tableMappings.GetAllTables().FirstOrDefault(x => x is TransposeStatsTableMapping);
            if (transposeTable != null)
            {
                using (new DebugTimer("DefaultSearchRequestMapper.FindTransposeStatsColumnsInCalculation"))
                {
                    foreach (var col in selectedColumns)
                    {
                        if (QueryHelpers.IsCalculatedColumn(col))
                        {
                            var foundColumns = GetNestedColumns(col);
                            var found = foundColumns.Where(x => x.Key.KnownTable == transposeTable.KnownTableName).ToList();

                            if (found.Any())
                            {
                                foreach (var foundCol in found)
                                {
                                    if (foundCol.Key.ActionSpecId.HasValue && result.All(x => x.Id != foundCol.Key.Id))
                                    {
                                        result.Add(foundCol.Key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private List<ReportColumnMapping> GetAllColumnsUsedByCalculatedColumns(List<ReportColumnMapping> allCalculatedColumns, bool isSummarizing)
        {
            var result = new List<ReportColumnMapping>();

            foreach (var calcCol in allCalculatedColumns)
            {

                var foundColumns = GetNestedColumns(calcCol);

                foreach (var foundColumn in foundColumns.Where(x => !x.Key.IsCalculated))
                {
                    if (result.All(x => x.Id != foundColumn.Key.Id))
                    {
                        result.Add(foundColumn.Key);
                    }
                }

                bool isCustomColumn = calcCol.OrganizationId > 0;
                if (isSummarizing || isCustomColumn)
                {
                    var fieldNameWithAlias = GetColumSelector(calcCol, useFieldAlias: true).FullName;
                    var foundAliasColumns = _calculatedColumnHelper.FindColumnsInCalculatedField(fieldNameWithAlias);

                    foreach (var foundColumn in foundAliasColumns)
                    {
                        if (result.All(x => x.Id != foundColumn.Key.Id))
                        {
                            result.Add(foundColumn.Key);
                        }
                    } 
                } 

            }

            return result;
        }

        protected virtual Dictionary<ReportColumnMapping, string> GetNestedColumns(ReportColumnMapping calcCol)
        {
            if (calcCol.NestedColumns == null)
            {
                // get the generated field name 
                var fieldName = GetColumSelector(calcCol).FullName;
                var foundColumns = _calculatedColumnHelper.FindColumnsInCalculatedField(fieldName);
                calcCol.NestedColumns = foundColumns;
            }
            return calcCol.NestedColumns;
        }

        protected virtual Column GetColumSelector(
         ReportColumnMapping col,
         bool dontAggregate = false,
         bool useFieldAlias = false)
        {
            // todo: this is a hack 
            return _dataSourceComponents.SearchQueryBuilder.GetColumnSelector(col, null, dontAggregate: dontAggregate, useFieldAlias: useFieldAlias);
        }
        

        public override List<MappedSearchRequestFilter> GetMappedFilters(List<SearchRequestFilter> filters)
        {
            return new SearchRequestFilterMapper(_columnProvider, _constants).Map(filters);
        }
    }
}
