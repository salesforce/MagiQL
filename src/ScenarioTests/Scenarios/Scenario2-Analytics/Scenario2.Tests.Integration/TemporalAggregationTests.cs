using System;
using MagiQL.Framework.Model;
using NUnit.Framework;
using Scenarios.Scenario2.Tests.Integration.Helpers;

namespace Scenarios.Scenario2.Tests.Integration
{
    [TestFixture]
    public class TemporalAggregationTests : TestBase
   {
        
        [TestCase("2000-01-01 00:00","2000-01-01 01:00", 0)]
        [TestCase("2000-01-01 00:00","2000-01-01 06:00", 2)]
        [TestCase("2000-01-01 00:00","2000-01-01 13:00", 5)]
        [TestCase("2000-01-01 00:00","2000-01-03 00:00", 15)]
        [TestCase("2000-01-01 00:00","2000-02-01 00:00", 25)]
        [TestCase(null,null, 25)]
        public void DateQuery_Ungrouped_AggregateByDay_ExcludeNoStats_ReturnsRows(string startDate, string endDate, int expectedRowCount)
        { 
            // arrange
            var searchRequest = SetupRequest(_client, "Location_ID");
            searchRequest.ExcludeRecordsWithNoStats = true;
            searchRequest.DateStart = startDate!=null ? DateTime.Parse(startDate) : (DateTime?)null;
            searchRequest.DateEnd = endDate!=null ?  DateTime.Parse(endDate) : (DateTime?)null;
            searchRequest.DateRangeType = DateRangeType.AccountTime;
            searchRequest.TemporalAggregation = TemporalAggregation.ByDay;

            // act
            var result = _client.Search(_platform, 1, 1, searchRequest);
             
            // assert 
            Assert.IsNull(result.Error);
            Assert.AreEqual(expectedRowCount, result.Data.Count);
        }

        [TestCase(1, "2000-01-01 00:00", "2000-01-02 23:59", "LocationHit_Count", false, 2)] 
        [TestCase(1, "2000-01-01 00:00", "2000-01-02 23:59", "LocationHit_Count", true, 1)]

        [TestCase(5, "2000-01-01 00:00", "2000-01-02 23:59", "LocationHit_Count", false, 4)]
        [TestCase(5, "2000-01-01 00:00", "2000-01-02 23:59", "LocationHit_Count", true, 3)]
        [TestCase(5, "2000-01-01 00:00", "2000-01-03 23:59", "LocationHit_Count", false, 4)]  
        [TestCase(5, "2000-01-01 00:00", "2000-01-03 23:59", "LocationHit_Count", true, 1)]  
        public void DateQuery_Ungrouped_ExcludeNoStats_AggregateByDay_ReturnsExpectedValueCount(int locationId, string startDate, string endDate, string assertColumnUniqueName, bool sortDescending, object expectedValue)
        {
            // arrange
            var searchRequest = SetupRequest(_client, "Location_ID", sortByColumnUniqueName: "LocationHit_TimeStampUTC");
            searchRequest.SortDescending = sortDescending;
            searchRequest.AddFilter(_allColumns, "Location_ID", locationId); 
            searchRequest.DateStart = DateTime.Parse(startDate);
            searchRequest.DateEnd = DateTime.Parse(endDate);
            searchRequest.DateRangeType = DateRangeType.AccountTime;
            searchRequest.TemporalAggregation = TemporalAggregation.ByDay;
            searchRequest.ExcludeRecordsWithNoStats = true;

            // act
            var result = _client.Search(_platform, 1, 1, searchRequest);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.IsNull(result.Error); 
            Assert.AreEqual(expectedValue, dataTable.Rows[0][assertColumnUniqueName]);
        }


    }

}
