using System;
using System.Collections.Generic;

namespace MagiQL.Framework.Model.Request
{
    public class SearchRequest
    {
        // filters
        [Obsolete("To be deprecated soon. Use TextFilter instead.")]
        public string Query { get; set; }
        public string TextFilter { get; set; }

        public List<SelectedColumn> TextFilterColumns { get; set; }

        public List<SearchRequestFilter> Filters { get; set; }

        // group by
        public SelectedColumn GroupByColumn { get; set; }

        /// <summary>
        /// Second level grouping - after filtering
        /// </summary>
        public SelectedColumn SummarizeByColumn { get; set; }

        // pagination
        public int PageIndex { get; set; }
        public int PageSize { get; set; } 
        public bool GetCount { get; set; }

        // sorting
        public SelectedColumn SortByColumn { get; set; }
        public bool SortDescending { get; set; }

        // column selection
        public List<SelectedColumn> SelectedColumns { get; set; }

        // date range & temporal aggregation

        /// <summary>
        /// The inclusive start date of the date range over which to query stats
        /// or null to query lifetime stats.
        /// 
        /// Must be in ISO 8601 format. 
        /// 
        /// If DateRangeType is 'Utc', it must include the UTC offset. E.g.:
        /// "2015-06-15T13:00Z",
        /// "2015-06-15T13:00+00:00", or
        /// "2015-06-15T16:00+03:00"
        /// 
        /// If DateRangeType is 'AccountTime', it must be a Local Date, i.e. a value
        /// without a UTC offset. E.g.
        /// "2015-06-15T13:00"
        /// </summary>
        public DateTime? DateStart { get; set; }

        /// <summary>
        /// The *inclusive* end date of the date range over which to query stats
        /// or null to query all the stats up until now.
        /// 
        /// Must be in ISO 8601 format. 
        /// 
        /// If DateRangeType is 'Utc', it must include the UTC offset. E.g.:
        /// "2015-06-15T13:00Z",
        /// "2015-06-15T13:00+00:00", or
        /// "2015-06-15T16:00+03:00"
        /// 
        /// If DateRangeType is 'AccountTime', it must be a Local Date, i.e. a value 
        /// expressed in an unspecified timezone, i.e. a value without a UTC offset.
        /// E.g.
        /// "2015-06-15T13:00"
        /// </summary>
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// Indicates whether the provided date range is expressed in UTC or in the ad account
        /// timezone.
        /// 
        /// For example: to query the stats of all the campaigns for the past
        /// 24 hours, use a UTC date range with DateStart set to UTC - 24 hours. 
        /// 
        /// To query the stats of all the campaigns for the 4 July in each ad account's
        /// local timezone, use an "AccountTime" date range with DateStart and DateEnd set to 
        /// 4 July. The returned stats will match what Facebook displays in their native UI
        /// as Facebook always display stats in the account's local timezone. 
        /// </summary>
        public DateRangeType DateRangeType { get; set; }

        /// <summary>
        /// Temporal aggregation is the final level of grouping  
        /// </summary>
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
