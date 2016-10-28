using System;
using System.Linq;
using MagiQL.Framework.Model.Request;
using NUnit.Framework;
using Scenarios.Scenario1.Tests.Integration.Helpers;

namespace Scenarios.Scenario1.Tests.Integration
{
    [TestFixture]
    public class PaginationTests : TestBase
    {
        [TestCase("Room_ID", false, 0, 1, 1, 1)]
        [TestCase("Room_ID", false, 0, 2, 1, 2)]
        [TestCase("Room_ID", false, 1, 1, 2, 1)]
        [TestCase("Room_ID", false, 1, 2, 3, 2)]
        [TestCase("Room_ID", false, 1, 4, 5, 4)]
        [TestCase("Room_ID", false, 1, 7, 8, 1)]
        [TestCase("Room_ID", false, 2, 3, 7, 2)]
        [TestCase("Room_ID", true, 0, 1, 8, 1)]
        [TestCase("Room_ID", true, 1, 1, 7, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", false, 0, 1, 18, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", false, 0, 2, 18, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", false, 1, 2, 20, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", false, 1, 4, 21, 4)]
        public void UngroupedRequest_SortByTestCase_Paginated_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            int pageIndex, int pageSize, object firstValue , int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.PageIndex = pageIndex;
            request.PageSize = pageSize;
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SortDescending = descending;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows[0][sortColumnUniqueName] is decimal)
            {
                firstValue = Math.Round(Convert.ToDecimal(firstValue), 7); 
            }
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Console.WriteLine(dataTable.Rows[0][sortColumnUniqueName]);
            Assert.AreEqual(firstValue, dataTable.Rows[0][sortColumnUniqueName]);
        }

        [TestCase("Room_HouseID", false, 0, 1, 1, 1)]
        [TestCase("Room_HouseID", false, 0, 2, 1, 2)]
        [TestCase("Room_HouseID", false, 1, 1, 2, 1)]
        [TestCase("Room_HouseID", false, 0, 10, 1, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", false, 0, 1, 18, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", false, 0, 2, 18, 2)] 
        [TestCase("RoomSensor_TemperatureCelcius", false, 1, 1, 20, 1)] 
        public void GroupedRequest_SortByTestCase_Paginated_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            int pageIndex, int pageSize, object firstValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.PageIndex = pageIndex;
            request.PageSize = pageSize;
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SortDescending = descending;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows[0][sortColumnUniqueName] is decimal)
            {
                firstValue = Math.Round(Convert.ToDecimal(firstValue), 7);
            }
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Console.WriteLine(dataTable.Rows[0][sortColumnUniqueName]);
            Assert.AreEqual(firstValue, dataTable.Rows[0][sortColumnUniqueName]);
        }
        

    }
}
