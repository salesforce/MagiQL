namespace MagiQL.Framework.Model
{
    /// <summary>
    /// Indicates how to aggregate the resulting stats.
    /// </summary>
    public enum  TemporalAggregation
    {
        /// <summary>
        /// Aggregate all the stats and return the total number across the specified date range.
        /// </summary>
        Total = 0,
        /// <summary>
        /// Aggregate the stats by day according to the timezone of the Ad Account
        /// in question.
        /// </summary>
        ByDay = 1440,
        /// <summary>
        /// Aggregate the stats by hour according to UTC time. 
        /// </summary>
        ByHour = 60
    }
}
