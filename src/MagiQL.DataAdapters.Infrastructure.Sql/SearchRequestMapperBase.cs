using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;

namespace MagiQL.DataAdapters.Infrastructure.Sql
{
    public abstract class SearchRequestMapperBase
    { 

        public MappedSearchRequest Map(SearchRequest request)
        {
            var result = new MappedSearchRequest()
            {
                DateEnd = request.DateEnd,
                DateStart = request.DateStart,
                DateRangeType = request.DateRangeType,
                DebugMode = request.DebugMode,
                GetCount = request.GetCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TextFilter = request.TextFilter ?? request.Query, // TODO: Remove Query.
                TemporalAggregation = request.TemporalAggregation,
                ExcludeRecordsWithNoStats = request.ExcludeRecordsWithNoStats,
                SortDescending = request.SortDescending,
            };

            result.GroupByColumn = GetColumnMapping(request.GroupByColumn);
            result.SummarizeByColumn = GetColumnMapping(request.SummarizeByColumn);
            result.SortByColumn = GetColumnMapping(request.SortByColumn);
            result.SelectedColumns = GetColumnMappings(request.SelectedColumns);
            result.TextFilterColumns = GetColumnMappings(request.TextFilterColumns);

            result.Filters = GetMappedFilters(request.Filters);

            result.DependantColumns = GetDependantColumnMappings(result);

            return result;
        }


        private List<ReportColumnMapping> GetColumnMappings(List<SelectedColumn> list)
        {
            if (list == null)
            {
                return new List<ReportColumnMapping>();
            }

            var result = list
                            .AsParallel()
                            .Select(GetColumnMapping)
                            .ToList();

            // hack todo: fix later
            foreach (var reportColumnMapping in result)
            {
                if (reportColumnMapping.FieldName == "COUNT()")
                {
                    reportColumnMapping.FieldName = "_C";
                    reportColumnMapping.IsCalculated = false;
                }
            }

            return result;
        }

        public abstract ReportColumnMapping GetColumnMapping(SelectedColumn column);

        public abstract List<ReportColumnMapping> GetDependantColumnMappings(MappedSearchRequest request);

        public abstract List<MappedSearchRequestFilter> GetMappedFilters(List<SearchRequestFilter> filters);

    }
}
