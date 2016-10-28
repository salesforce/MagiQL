using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Model
{
    // shit name, we can rename it later
    public class MappedSearchRequest
    {
        // filters
        public string TextFilter { get; set; }
        public List<ReportColumnMapping> TextFilterColumns { get; set; }
        public List<MappedSearchRequestFilter> Filters { get; set; }

        // group by
        public ReportColumnMapping GroupByColumn { get; set; }

        // summarize by (todo)
        public ReportColumnMapping SummarizeByColumn { get; set; }

        // pagination
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool GetCount { get; set; }

        // sorting
        public ReportColumnMapping SortByColumn { get; set; }
        public bool SortDescending { get; set; }

        // column selection
        public List<ReportColumnMapping> SelectedColumns { get; set; }
        
        /// <summary>
        /// Columns which are not selected but are needed for calculated columns
        /// </summary>
        public List<ReportColumnMapping> DependantColumns { get; set; }

        /// <summary>
        /// A union on SelectedColumns and DependantColumns
        /// </summary>
        public List<ReportColumnMapping> SelectedAndDependantColumns {
            get { return SelectedColumns.Union(DependantColumns).ToList(); }
        }

        // date range & resolution
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public DateRangeType DateRangeType { get; set; }

        public TemporalAggregation TemporalAggregation { get; set; }

        /// <summary>
        /// When set, objects that don't have any corresponding stats records will be excluded
        /// from the results.
        /// </summary>
        public bool ExcludeRecordsWithNoStats { get; set; }

        /// <summary>
        /// When enabled, returns the sql and the request and includes the column names inside the data
        /// </summary>
        public bool DebugMode { get; set; }
    }
}
