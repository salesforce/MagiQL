namespace MagiQL.Framework.Model
{
    public enum DateRangeType
    {
        /// <summary>
        /// The date/time values in the provided date range are UTC dates (i.e. each value represents 
        /// an instant in time). 
        /// </summary>
        Utc = 1,
        /// <summary>
        /// The date/time values in the provided date range are expressed in the Ad Account Timezone 
        /// (i.e. they will correspond to a different instant in time dependending on the timezone
        /// that the ad account in question uses). 
        /// </summary>
        AccountTime = 2
    }
}
