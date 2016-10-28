using System;
using MagiQL.Framework.Model;
using SqlModeller.Model;
using SqlModeller.Shorthand;

namespace MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Stats
{
    public static class StatsQueryHelpers
    { 

        /// <summary>
        /// Applies the appropriate WHERE clauses to restrict the query to the provided date range.
        /// </summary>
        public static void AddDateFilters(
            SelectQuery query, 
            string statsTableAlias, 
            TemporalAggregation temporalAggregation,
            DateRangeType dateRangeType,
            string dateTimeField,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (String.IsNullOrWhiteSpace(statsTableAlias)) throw new ArgumentException("Invalid value provided for 'statsTableAlias'. 'statsTableAlias' cannot be null or empty.");
            if (endDate.HasValue && !startDate.HasValue) throw new ArgumentException("Invalid value provided for startDate and endDate. An endDate was provided but startDate was null. When an endDate is provided, a startDate must also be provided.");

            if (!startDate.HasValue) // No date filtering
            {
                return;
            } 

            query.Where(Combine.And);

            if (temporalAggregation == TemporalAggregation.ByHour ||
                (temporalAggregation == TemporalAggregation.Total && dateRangeType == DateRangeType.Utc))
            {
                // We're querying the Hourly stats table.
                //
                // The *Hourly tables should store both the UTC hour (in the DateTime column) and the Account Timezone Hour 
                // (in the LocalTime column) for each record. So we can select using either a UTC date range or a date range
                // expressed in the account timezone.
                //
                // IMPORTANT NOTE: in the *Hourly tables, stats are stored against the *end value* of the date range they cover.
                // E.g. the stats for hour 13:00 -> 14:00 will be stored against 14:00.
                //
                // So we must adjust the date range we've been asked to retrieve to fit with the way 
                // our data is stored.

                var hourlyStartDbFormat = startDate.Value.AddHours(1);

                if (dateRangeType == DateRangeType.Utc)
                {
                    query.WhereColumnValue(statsTableAlias, dateTimeField, Compare.GreaterThanOrEqual, hourlyStartDbFormat);
                }
                else if (dateRangeType == DateRangeType.AccountTime)
                {
                    query.WhereColumnValue(statsTableAlias, "LocalTime", Compare.GreaterThanOrEqual, hourlyStartDbFormat);
                }
                else
                {
                    throw new ArgumentException(String.Format("Unexpected value provided for request.DateRangeType: [{0}]. Don't know how to handle this type of date range.", dateRangeType));
                }
            }
            else
            {
                // We're querying the Daily stats table. 
                query.WhereColumnValue(statsTableAlias, dateTimeField, Compare.GreaterThanOrEqual, startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                if (temporalAggregation == TemporalAggregation.ByHour ||
                    (temporalAggregation == TemporalAggregation.Total && dateRangeType == DateRangeType.Utc))
                {
                    var hourlyEndDbFormat = endDate.Value.AddHours(1);

                    if (dateRangeType == DateRangeType.Utc)
                    {
                        query.WhereColumnValue(statsTableAlias, dateTimeField, Compare.LessThanOrEqual, hourlyEndDbFormat);
                    }
                    else if (dateRangeType == DateRangeType.AccountTime)
                    {
                        query.WhereColumnValue(statsTableAlias, "LocalTime", Compare.LessThanOrEqual, hourlyEndDbFormat);
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("Unexpected value provided for request.DateRangeType: [{0}]. Don't know how to handle this type of date range.", dateRangeType));
                    }
                }
                else
                {
                    query.WhereColumnValue(statsTableAlias, dateTimeField, Compare.LessThanOrEqual, endDate.Value);
                }
            }
        }

        /// <summary>
        /// Add the appropriate GROUP BY clause to the provided query to group the 
        /// stats data by the requested temporal aggregation.
        /// </summary>
        public static void AddTemporalAggregationGroupBy(SelectQuery query, TemporalAggregation temporalAggregation, string dateTimeField)
        {

            if (temporalAggregation == TemporalAggregation.ByDay)
            {
                query.GroupByDatePart(string.Empty, dateTimeField, DatePart.Day); // This is the day expressed in the account timezone (not UTC)
            }
            else if (temporalAggregation == TemporalAggregation.ByHour)
            {
                // The records in our Hourly stats tables include both the hour expressed in UTC ("DateTime" column) and the
                // hour expressed in the account timezone ("LocalTime" column). The UTC hour is the reference however. It's the
                // value that indicates the hour when those stats were calculated. It's guaranteed to be contiguous (i.e. no holes
                // and no duplicates). Due to DST timezone switches, the "LocalTime" values may have holes as well as 
                // duplicate records. This is normal and expected. But it means that grouping must always be done using the UTC
                // hour to avoid getting some funky results around DST timezone changes. 
                query.GroupByDatePart(string.Empty, dateTimeField, DatePart.Hour);
            }
        }
    }
}
