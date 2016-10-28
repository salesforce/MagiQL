using System;
using System.Linq;
using MagiQL.Framework.Model.Request;
using NUnit.Framework;
using Scenarios.Scenario1.Tests.Integration.Helpers;

namespace Scenarios.Scenario1.Tests.Integration
{
    [TestFixture]
    public class SortByTests : TestBase
    {
        [TestCase("Room_ID", false, 1, 8)]
        [TestCase("Room_ID", true, 8, 1)]
        [TestCase("Room_HouseID", false, 1, 2)]
        [TestCase("Room_HouseID", true, 2, 1)] 
        [TestCase("Room_VolumeCubicMetres", false, 10, 45)]
        [TestCase("Room_VolumeCubicMetres", true, 45, 10)]
        [TestCase("RoomSensor_HumidityPercent", false, 28.8, 39.1)] 
        [TestCase("RoomSensor_HumidityPercent", true, 39.1, 28.8)] 
        [TestCase("RoomSensor_TemperatureCelcius", false, 18, 26)]
        [TestCase("RoomSensor_TemperatureCelcius", true, 26, 18)]
        [TestCase("Room_Name", false, "Bedroom 1", "Living Room")]
        [TestCase("Room_Name", true, "Living Room", "Bedroom 1")] 
        [TestCase("RoomSensor_IsLight", false, false, true)]
        [TestCase("RoomSensor_IsLight", true, true, false)] 
        public void UngroupedRequest_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            object firstValue, object lastValue)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SortDescending = descending;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows[0][sortColumnUniqueName] is decimal)
            {
                firstValue = Math.Round(Convert.ToDecimal(firstValue), 7);
                lastValue = Math.Round(Convert.ToDecimal(lastValue), 7);
            }
            Assert.AreEqual(8, dataTable.Rows.Count);

            var firstRow = dataTable.Rows[0][sortColumnUniqueName];
            if (firstRow is decimal)
            {
                firstRow = Math.Round((decimal)firstRow, 7);
            }
            Assert.AreEqual(firstValue, firstRow);

            var lastRow = dataTable.Rows[dataTable.Rows.Count - 1][sortColumnUniqueName];
            if (lastRow is decimal)
            {
                lastRow = Math.Round((decimal)lastRow, 7);
            } 
            Assert.AreEqual(lastValue, lastRow);
        }

        [TestCase("Room_HouseID", false, 1, 2)]
        [TestCase("Room_HouseID", true, 2, 1)]
        [TestCase("Room_Count", true, 5, 3)]
        [TestCase("RoomSensor_Count", false, 3, 5)]
        [TestCase("Room_VolumeCubicMetres", false, 10, 20)]
        [TestCase("Room_VolumeCubicMetres_Max", true, 45, 35)]
        [TestCase("Room_VolumeCubicMetres_Sum", true, 125, 105)]
        [TestCase("RoomSensor_HumidityPercent", false, 28.8, 33.7)] 
        [TestCase("RoomSensor_HumidityPercent_Max", true, 39.1, 37.2)] 
        [TestCase("RoomSensor_TemperatureCelcius", false, 18, 20)]
        [TestCase("RoomSensor_TemperatureCelcius_Max", true, 26, 24)]  
        public void GroupedRequest_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            object firstValue, object lastValue)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SortDescending = descending;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows[0][sortColumnUniqueName] is decimal)
            {
                firstValue = Convert.ToDecimal(firstValue);
                lastValue = Convert.ToDecimal(lastValue);
            }

            Assert.AreEqual(2, dataTable.Rows.Count);
            Assert.AreEqual(firstValue, dataTable.Rows[0][sortColumnUniqueName]);
            Assert.AreEqual(lastValue, dataTable.Rows[dataTable.Rows.Count - 1][sortColumnUniqueName]);
        }
        
        [TestCase("Room_HouseID", false, 1, 2)]
        [TestCase("Room_HouseID", true, 2, 1)]
        [TestCase("Room_Count", true, 5, 3)]
        [TestCase("RoomSensor_Count", false, 3, 5)]
        [TestCase("Room_VolumeCubicMetres", false, 10, 20)]
        [TestCase("Room_VolumeCubicMetres_Max", true, 45, 35)]
        [TestCase("Room_VolumeCubicMetres_Sum", true, 125, 105)]
        [TestCase("RoomSensor_HumidityPercent", false, 28.8, 33.7)]
        [TestCase("RoomSensor_HumidityPercent_Max", true, 39.1, 37.2)]
        [TestCase("RoomSensor_TemperatureCelcius", false, 18, 20)]
        [TestCase("RoomSensor_TemperatureCelcius_Max", true, 26, 24)]
        public void SummarizedRequest_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            object firstValue, object lastValue)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.SummarizeByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == "Room_HouseID").Id);
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SortDescending = descending;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows[0][sortColumnUniqueName] is decimal)
            {
                firstValue = Convert.ToDecimal(firstValue);
                lastValue = Convert.ToDecimal(lastValue);
            }

            Assert.AreEqual(2, dataTable.Rows.Count);
            Assert.AreEqual(firstValue, dataTable.Rows[0][sortColumnUniqueName]);
            Assert.AreEqual(lastValue, dataTable.Rows[dataTable.Rows.Count - 1][sortColumnUniqueName]);
        }


    }
}
