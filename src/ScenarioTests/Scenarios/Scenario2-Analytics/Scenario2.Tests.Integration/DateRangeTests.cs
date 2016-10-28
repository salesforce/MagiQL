using System;
using MagiQL.Framework.Model;
using NUnit.Framework;
using Scenarios.Scenario2.Tests.Integration.Helpers;

namespace Scenarios.Scenario2.Tests.Integration
{
    [TestFixture]
    public class DateRangeTests : TestBase
   {
        
        [TestCase("2000-01-01 00:00","2000-01-01 01:00", 0)]
        [TestCase("2000-01-01 00:00","2000-01-01 06:00", 2)]
        [TestCase("2000-01-01 00:00","2000-01-01 13:00", 5)]
        [TestCase("2000-01-01 07:00", "2000-01-01 13:00", 3)] 
        [TestCase("2000-01-01 00:00", "2000-01-03 00:00", 10)] 
        public void DateQuery_Ungrouped_ExcludeNoStats_ReturnsRows(string startDate, string endDate, int expectedRowCount)
        { 
            // arrange
            var searchRequest = SetupRequest(_client, "Location_ID");
            searchRequest.ExcludeRecordsWithNoStats = true;
            searchRequest.DateStart = DateTime.Parse(startDate);
            searchRequest.DateEnd = DateTime.Parse(endDate);
            searchRequest.DateRangeType = DateRangeType.AccountTime;

            // act
            var result = _client.Search(_platform, 1, 1, searchRequest);
             
            // assert 
            Assert.IsNull(result.Error);
            Assert.AreEqual(expectedRowCount, result.Data.Count);
        }
          
        [TestCase(1, "2000-01-01 12:00", "2000-01-01 12:00", "LocationHit_Count", 1)]
        [TestCase(1, "2000-01-01 12:00", "2000-01-01 12:05", "LocationHit_Count", 2)]
        [TestCase(1, "2000-01-01 12:00", "2000-01-02 14:54", "LocationHit_Count", 2)]
        [TestCase(1, "2000-01-01 12:00", "2000-01-02 14:55", "LocationHit_Count", 3)] 

        [TestCase(5, "2000-01-01 12:00", "2000-01-01 12:00", "LocationHit_Count", 1)]
        [TestCase(5, "2000-01-01 12:00", "2000-01-01 12:05", "LocationHit_Count", 2)]
        [TestCase(5, "2000-01-01 12:00", "2000-01-01 12:11", "LocationHit_Count", 3)] 
        [TestCase(5, "2000-01-01 12:00", "2000-01-02 14:55", "LocationHit_Count", 7)]
        [TestCase(5, "2000-01-01 12:00", "2000-01-03 19:38", "LocationHit_Count", 8)]

        //[TestCase(7, "2000-01-01 18:35", "2000-01-01 18:35", "LocationHit_ResponseSizeBytes_Sum", 2000)] 
        //[TestCase(7, "2000-01-01 18:35", "2000-01-01 18:36", "LocationHit_ResponseSizeBytes_Sum", 4000)] // known issue, date stats can only expose a column with 1 aggregation
        //[TestCase(7, "2000-01-01 18:35", "2000-01-01 22:12", "LocationHit_ResponseSizeBytes_Sum", 6000)] 
        public void DateQuery_Ungrouped_ExcludeNoStats_ReturnsExpectedValueCount(int locationId, string startDate, string endDate, string assertColumnUniqueName, object expectedValue)
        {
            // arrange
            var searchRequest = SetupRequest(_client, "Location_ID");
            searchRequest.AddFilter(_allColumns, "Location_ID", locationId); 
            searchRequest.DateStart = DateTime.Parse(startDate);
            searchRequest.DateEnd = DateTime.Parse(endDate);
            searchRequest.DateRangeType = DateRangeType.AccountTime;

            // act
            var result = _client.Search(_platform, 1, 1, searchRequest);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.IsNull(result.Error);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(expectedValue, dataTable.Rows[0][assertColumnUniqueName]);
        }


        // todo : grouped queries
    }

}
