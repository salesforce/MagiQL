using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.DataAdapters.Infrastructure.Sql;

namespace MagiQL.Framework.Validation
{
    public class SearchRequestValidator : ISearchRequestValidator
    {
        private readonly IReportsDataSourceFactory _reportsDataSourceFactory; 

        public SearchRequestValidator(IReportsDataSourceFactory reportsDataSourceFactory)
        {
            this._reportsDataSourceFactory = reportsDataSourceFactory; 
        }

        public void Validate(string platform, int? organizationId, SearchRequest request)
        {
            ValidateDateRange(request.DateStart, request.DateEnd, request.DateRangeType, request.TemporalAggregation);

            var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
            var knownColumns = dataSource.GetAllSelectableColumnDefinitions(organizationId);
              
            ValidateColumns(platform, request, knownColumns);
            ValidateQueryColumns(request, knownColumns, dataSource);

            if (request.SortByColumn == null || request.SortByColumn.ColumnId == 0)
            {
                throw new Exception("SortByColumn cannot be null");
            }

            ValidateGroupBy(request, knownColumns);
        }

        private void ValidateQueryColumns(
            SearchRequest request,
            List<ColumnDefinition> knownColumns,
            IReportsDataSource dataSource)
        {
            var notFoundColumns = new List<SelectedColumn>();

            // check selected columns
            if (request.TextFilterColumns == null)
            {
                return;
            }

            // check if columns exist
            foreach (var selectedColumn in request.TextFilterColumns)
            {
                if (knownColumns.All(x => x.Id != selectedColumn.ColumnId))
                {
                    notFoundColumns.Add(selectedColumn);
                }
            }

            if (notFoundColumns.Any())
            {
                string errorMsg = "Requested query columns not recognised : ";
                foreach (var column in notFoundColumns)
                {
                    errorMsg += string.Format(" {0},", column.ColumnId);
                }
                errorMsg.TrimEnd(',');

                throw new Exception(errorMsg);
            }

            var columnMappings = dataSource.GetColumnMappings(request.TextFilterColumns);

            foreach (var selectedColumn in columnMappings)
            {
                if (!selectedColumn.DbType.IsStringType())
                {
                    throw new Exception("At least one of the requested text search query columns is not of type String.");
                }
            }
        }

        private void ValidateDateRange(DateTime? dateStart, DateTime? dateEnd, DateRangeType dateRangeType, TemporalAggregation temporalAggregation)
        {
            // If no date range has been specified - we're good. We're querying lifetimes.
            if (!dateStart.HasValue && !dateEnd.HasValue)
            {
                return;
            }

            // A date range must have a start date
            if (!dateStart.HasValue)
            {
                throw new Exception(String.Format("Invalid date range: an end date was provided ([{0}]) but the start date was missing. If you specify an end date, you must also specify a start date. If you wish to query lifetime stats, omit both the start and the end date.", dateEnd.Value.ToString("yyyy-MM-dd HH:mm")));
            }

            // The end date can't be earlier than the start date 
            // (the date range is inclusive/inclusive so it's valid to have start == end)
            if (dateEnd.HasValue && dateEnd.Value < dateStart.Value)
            {
                throw new Exception(String.Format("Invalid date range: [{0}] -> [{1}]. The end date must be greater than or equal to the start date. If you wish to query stats up until now, you can omit the end date altogether.",
                    dateStart.Value.ToString("yyyy-MM-dd HH:mm"),
                    dateEnd.Value.ToString("yyyy-MM-dd HH:mm")));
            }

            // We don't allow querying daily stats using a UTC date range 
            // (because our daily stats are stored against the day expressed in the ad account timezone,
            // which means that we can't query them for a UTC date range).
            if (dateRangeType == DateRangeType.Utc && temporalAggregation != TemporalAggregation.ByHour && temporalAggregation != TemporalAggregation.Total)
            {
                throw new Exception(String.Format("Invalid combination of dateRangeType ([{0}]) and temporalAggregation ([{1}]). Express your date range in AccountTime instead. Unfortunately, due to the limitations of our stats data set, we are unable to query daily stats using a UTC date range.", dateRangeType, temporalAggregation));
            }

            // Check that the dates are UTC dates if the date range was meant to be a UTC date range 
            // or are in an unspecified timezone if the date range was meant to be in account time. 

            /* 
             * This is really just to catch buggy upstream code. We could of course just force the DateTime Kind value to whatever
             * we need it to be. But if someone said they were sending us a date range in Account Time and yet specified their dates 
             * as absolute instants in time (or vice-versa), it most likely means that there is something wrong 
             * in the way they handle date/time values. Which means that there is a good chance that the values they sent us will be off.
             * Rather than being tolerant and ending up in a huge mess in a few months time when people start to complain that dates are
             * off-by-one due to a buggy client that doesn't handle their dates properly, enfore a very strict contract for the Reporting
             * service when it comes to date/time values. It will force clients to be explicit when handling and serializing date/times 
             * instead of relying on auto-magical behaviour (which, when is comes to date and times, always leads to a huge mess).
             */

            // Refer to our JSON.NET config in the WebApiConfig class of the Reporting Service WebAPI project for all the details
            // on how we deserialize date/time values.

            if (dateRangeType == DateRangeType.Utc)
            {
                if (dateStart.Value.Kind != DateTimeKind.Utc)
                {
                    throw new Exception("Invalid value provided for dateStart. The provided value was missing its UTC offset. When specifying a dateRangeType of 'Utc', you must include the UTC offset in your date value. For example: '2015-06-19T13:00Z' or '2015-06-19T13:00+00:00' or '2015-06-19T15:00+02:00'. Otherwise, we have no way to know what instant in time the date you provided is supposed to represent.");
                }

                if (dateEnd.HasValue && dateEnd.Value.Kind != DateTimeKind.Utc)
                {
                    throw new Exception("Invalid value provided for dateEnd. The provided value was missing its UTC offset. When specifying a dateRangeType of 'Utc', you must include the UTC offset in your date value. For example: '2015-06-19T13:00Z' or '2015-06-19T13:00+00:00' or '2015-06-19T15:00+02:00'. Otherwise, we have no way to know what instant in time the date you provided is supposed to represent.");
                }
            }
            else if (dateRangeType == DateRangeType.AccountTime)
            {
                if (dateStart.Value.Kind != DateTimeKind.Unspecified)
                {
                    throw new Exception("Invalid value provided for dateStart. Was expecting to receive a Local Date, i.e. a date value with no UTC offset provided. But got an absolute date instead (a date with a UTC offset provided). When specifying a dateRangeType of 'AccountTime', you must specify your date values as Local Date (as per the ISO 8601 definition). I.e. you must not specify a UTC offset. For example: '2015-06-19T13:00'. This is because different ad accounts have different timezones, i.e. the timezone is unspecified. Specifying the UTC offset of such a date doesn't make sense and would be misleading.");
                }

                if (dateEnd.HasValue && dateEnd.Value.Kind != DateTimeKind.Unspecified)
                {
                    throw new Exception("Invalid value provided for dateStart. Was expecting to receive a Local Date, i.e. a date value with no UTC offset provided. But got an absolute date instead (a date with a UTC offset provided). When specifying a dateRangeType of 'AccountTime', you must specify your date values as Local Date (as per the ISO 8601 definition). I.e. you must not specify a UTC offset. For example: '2015-06-19T13:00'. This is because different ad accounts have different timezones, i.e. the timezone is unspecified. Specifying the UTC offset of such a date doesn't make sense and would be misleading.");
                }
            }
            else
            {
                throw new Exception("Unknown value provided for dateRangeType: " + dateRangeType);
            }
        }

        private static void ValidateGroupBy(SearchRequest request, List<ColumnDefinition> knownColumns)
        {
            if (request.GroupByColumn == null || request.GroupByColumn.ColumnId == 0)
            {
                throw new Exception("GroupByColumn cannot be null");
            }

            // if its not a data column, then it cannot be grouped

            var column = knownColumns.SingleOrDefault(x => x.Id == request.GroupByColumn.ColumnId);

            if (column == null)
            {
                string errorMsg = "Group by column not recognised : " + column.Id;
                throw new Exception(errorMsg);
            }

            if (!column.CanGroupBy)
            {
                string errorMsg = "Cannot group by column : " + column.DisplayName;
                throw new Exception(errorMsg);
            }
        }

        private void ValidateColumns(string platform, SearchRequest request, List<ColumnDefinition> knownColumns)
        {
            var notFoundColumns = new List<SelectedColumn>();

            // check selected columns
            if (request.SelectedColumns == null)
            {
                throw new Exception("No columns selected");
            }

            // sort by column
            if (knownColumns.All(x => x.Id != request.SortByColumn.ColumnId))
            {
                notFoundColumns.Add(request.SortByColumn);
            }

            // group by column
            if (knownColumns.All(x => x.Id != request.GroupByColumn.ColumnId))
            {
                notFoundColumns.Add(request.GroupByColumn);
            }


            // selected columns
            foreach (var selectedColumn in request.SelectedColumns)
            {
                if (knownColumns.All(x => x.Id != selectedColumn.ColumnId))
                {
                    notFoundColumns.Add(selectedColumn);
                }
            }


            if (notFoundColumns.Any())
            {
                string errorMsg = "Requested columns not recognised : ";
                foreach (var column in notFoundColumns)
                {
                    errorMsg += string.Format(" {0},", column.ColumnId);
                }
                errorMsg.TrimEnd(',');
                
                throw new Exception(errorMsg);
            }
        }
    }
}