using System;
using System.Data;
using System.Linq;
using MagiQL.Framework.Model.Request;
using NUnit.Framework;
using Scenarios.Scenario1.Tests.Integration.Helpers;

namespace Scenarios.Scenario1.Tests.Integration
{
    [TestFixture]
    public class FilterTests : TestBase
    {
        #region Ungrouped

        [TestCase("Room_ID",2,1)]
        [TestCase("Room_HouseID",2,3)]
        [TestCase("Room_VolumeCubicMetres", 40, 1)]
        [TestCase("Room_VolumeCubicMetres", 99, 0)]
        [TestCase("RoomSensor_HumidityPercent", 33.70, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 21, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 9, 0)]
        [TestCase("Room_Name", "Kitchen", 2)]
        [TestCase("Room_Name", "Unknown", 0)]
        [TestCase("RoomSensor_IsLight", true, 4)]
        public void Filter_Ungrouped_Equals_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue); 

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows.Count > 0 && dataTable.Rows[0][filterColumnUniqueName] is decimal)
            {
                filterValue = Convert.ToDecimal(filterValue);
            }

            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Assert.AreEqual(filterValue, dataRow[filterColumnUniqueName]);
            }
        }


        [TestCase("Room_ID", 2, 3, 2)]
        [TestCase("Room_ID", 1, 13, 1)]
        [TestCase("Room_HouseID", 1, 2, 8)]
        [TestCase("Room_VolumeCubicMetres", 40, 35, 2)] 
        [TestCase("RoomSensor_HumidityPercent", 33.70, 37.4, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 21, 90, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 9, 10, 0)] 
        [TestCase("RoomSensor_IsLight", true, false, 8)]
        public void Filter_Ungrouped_EqualsMultiValue_ReturnsExectedCount(string filterColumnUniqueName, object filterValue1, object filterValue2, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue1);
            request.Filters.First().Values.Add(filterValue2.ToString());

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert  
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error); 
        }


        [TestCase("Room_ID",2,7)]
        [TestCase("Room_HouseID",2,5)]
        [TestCase("Room_VolumeCubicMetres", 40, 7)]
        [TestCase("Room_VolumeCubicMetres", 99, 8)]
        [TestCase("RoomSensor_HumidityPercent", 33.70, 7)]
        [TestCase("RoomSensor_TemperatureCelcius", 21, 7)]
        [TestCase("RoomSensor_TemperatureCelcius", 9, 8)]
        [TestCase("Room_Name", "Kitchen", 6)]
        [TestCase("Room_Name", "Unknown", 8)]
        [TestCase("RoomSensor_IsLight", true, 4)]
        public void Filter_Ungrouped_NotEqual_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.NotEqual); 

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows.Count > 0 && dataTable.Rows[0][filterColumnUniqueName] is decimal)
            {
                filterValue = Convert.ToDecimal(filterValue);
            }

            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Assert.AreNotEqual(filterValue, dataRow[filterColumnUniqueName]);
            }
        }


        [TestCase("Room_ID", 2, 1)]
        [TestCase("Room_HouseID", 2, 5)]
        [TestCase("Room_VolumeCubicMetres", 25.1, 4)]
        [TestCase("Room_VolumeCubicMetres", 99, 8)]
        [TestCase("RoomSensor_HumidityPercent", 28.9, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 1)] 
        [TestCase("RoomSensor_TemperatureCelcius", 18, 0)] 
        public void Filter_Ungrouped_LessThan_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.LessThan);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }


        [TestCase("Room_ID", 2, 2)]
        [TestCase("Room_HouseID", 2, 8)]
        [TestCase("Room_VolumeCubicMetres", 25.1, 4)]
        [TestCase("Room_VolumeCubicMetres", 99, 8)]
        [TestCase("RoomSensor_HumidityPercent", 28.9, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 18, 1)]
        public void Filter_Ungrouped_LessThanOrEqual_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.LessThanOrEqual);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }


        [TestCase("Room_ID", 2, 6)]
        [TestCase("Room_HouseID", 2, 0)]
        [TestCase("Room_VolumeCubicMetres", 25.1, 4)]
        [TestCase("Room_VolumeCubicMetres", 99, 0)]
        [TestCase("RoomSensor_HumidityPercent", 28.9, 6)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 6)]
        [TestCase("RoomSensor_TemperatureCelcius", 18, 7)]
        public void Filter_Ungrouped_GreaterThan_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.GreaterThan);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }

        [TestCase("Room_ID", 2, 7)]
        [TestCase("Room_HouseID", 2, 3)]
        [TestCase("Room_VolumeCubicMetres", 25.1, 4)]
        [TestCase("Room_VolumeCubicMetres", 99, 0)]
        [TestCase("RoomSensor_HumidityPercent", 28.9, 7)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 7)]
        [TestCase("RoomSensor_TemperatureCelcius", 18, 8)]
        public void Filter_Ungrouped_GreaterThanOrEqual_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.GreaterThanOrEqual);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }


        [TestCase("Room_ID", 2, 5, 2)]
        [TestCase("Room_HouseID", 1, 3, 3)]
        [TestCase("Room_VolumeCubicMetres", 30, 50.1, 3)]
        [TestCase("Room_VolumeCubicMetres", 99, 100, 0)]
        [TestCase("RoomSensor_HumidityPercent", 28.8, 29.0, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 22, 3)]
        [TestCase("RoomSensor_TemperatureCelcius", 17, 19, 1)]
        public void Filter_Ungrouped_Between_ReturnsExectedCount(string filterColumnUniqueName, object filterValue1, object filterValue2, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue1, FilterModeEnum.GreaterThan);
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue2, FilterModeEnum.LessThan);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }


        #endregion

        #region Grouped


        [TestCase("Room_HouseID", 2, 1)]
        [TestCase("Room_Count", 5, 1)]
        [TestCase("RoomSensor_Count", 5, 1)]
        [TestCase("Room_VolumeCubicMetres_Max", 35, 1)]
        [TestCase("Room_VolumeCubicMetres_Sum", 99, 0)]
        [TestCase("RoomSensor_HumidityPercent_Max", 33.70, 0)]
        [TestCase("RoomSensor_HumidityPercent_Max", 39.1, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 22, 0)]
        [TestCase("RoomSensor_TemperatureCelcius", 18, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 9, 0)]
        [TestCase("Room_Name", "Bedroom 1", 2)]
        [TestCase("Room_Name", "Unknown", 0)]
        [TestCase("RoomSensor_IsLight", true, 0)]
        public void Filter_Grouped_Equals_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows.Count > 0 && dataTable.Rows[0][filterColumnUniqueName] is decimal)
            {
                filterValue = Convert.ToDecimal(filterValue);
            }

            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Assert.AreEqual(filterValue, dataRow[filterColumnUniqueName]);
            }
        }

        [TestCase("Room_Count", 5, 3, 2)]
        [TestCase("RoomSensor_Count", 5, 3, 2)]
        [TestCase("Room_HouseID", 2, 3, 1)]
        [TestCase("Room_HouseID", 2, 1, 2)]
        [TestCase("Room_VolumeCubicMetres_Max", 35, 36, 1)]
        [TestCase("Room_VolumeCubicMetres_Sum", 99, 100, 0)]
        [TestCase("RoomSensor_HumidityPercent_Max", 33.70, 34, 0)]
        [TestCase("RoomSensor_HumidityPercent_Max", 39.1, 40, 1)] 
        [TestCase("RoomSensor_TemperatureCelcius", 18, 19, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 9, 100, 0)]
        [TestCase("Room_Name", "Bedroom 1", "Bedroom 2", 2)]
        [TestCase("Room_Name", "Unknown", "Bedroom 1", 2)]
        [TestCase("RoomSensor_IsLight", true, false, 2)]
        public void Filter_Grouped_EqualsMultiValue_ReturnsExectedCount(string filterColumnUniqueName, object filterValue1, object filterValue2, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue1);
            request.Filters.First().Values.Add(filterValue2.ToString());

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert  
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }


        [TestCase("Room_Count", 5, 1)]
        [TestCase("Room_Count", 6, 2)]
        [TestCase("RoomSensor_Count", 6, 2)]
        [TestCase("Room_HouseID", 2, 1)]
        [TestCase("Room_VolumeCubicMetres_Max", 35, 1)]
        [TestCase("Room_VolumeCubicMetres_Sum", 99, 2)]
        [TestCase("RoomSensor_HumidityPercent_Max", 33.70, 2)]  
        [TestCase("RoomSensor_TemperatureCelcius", 9, 2)]
        [TestCase("Room_Name", "Bedroom 1", 0)]
        [TestCase("Room_Name", "Unknown", 2)]
        [TestCase("RoomSensor_IsLight", true, 2)]
        public void Filter_Grouped_NotEqual_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.NotEqual);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows.Count > 0 && dataTable.Rows[0][filterColumnUniqueName] is decimal)
            {
                filterValue = Convert.ToDecimal(filterValue);
            }

            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Assert.AreNotEqual(filterValue, dataRow[filterColumnUniqueName]);
            }
        }


        [TestCase("Room_Count", 5, 1)]
        [TestCase("Room_Count", 6, 2)]
        [TestCase("RoomSensor_Count", 5, 1)]
        [TestCase("Room_HouseID", 2, 1)]
        [TestCase("Room_VolumeCubicMetres_Max", 36, 1)]
        [TestCase("Room_VolumeCubicMetres_Sum", 125, 1)]
        [TestCase("RoomSensor_HumidityPercent_Max", 39.1, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 23, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 18, 0)]   
        public void Filter_Grouped_LessThan_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.LessThan);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }


        [TestCase("Room_Count", 3, 1)]
        [TestCase("Room_Count", 5, 2)]
        [TestCase("RoomSensor_Count", 5, 2)]
        [TestCase("Room_HouseID", 2, 2)]
        [TestCase("Room_VolumeCubicMetres_Max", 36, 1)]
        [TestCase("Room_VolumeCubicMetres_Sum", 125, 2)]
        [TestCase("RoomSensor_HumidityPercent_Max", 39.1, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 20, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 18, 1)]   
        public void Filter_Grouped_LessThanOrEqual_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.LessThanOrEqual);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }



        [TestCase("Room_Count", 3, 1)]
        [TestCase("Room_Count", 5, 0)]
        [TestCase("RoomSensor_Count", 3, 1)]
        [TestCase("Room_HouseID", 1, 1)]
        [TestCase("Room_HouseID", 0, 2)]
        [TestCase("Room_VolumeCubicMetres_Max", 36, 1)]
        [TestCase("Room_VolumeCubicMetres_Sum", 105, 1)]
        [TestCase("RoomSensor_HumidityPercent_Max", 37.1, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 17, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 18, 1)]   
        public void Filter_Grouped_GreaterThan_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.GreaterThan);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }


        [TestCase("Room_Count", 3, 2)]
        [TestCase("Room_Count", 5, 1)]
        [TestCase("RoomSensor_Count", 3, 2)]
        [TestCase("Room_HouseID", 1, 2)]
        [TestCase("Room_HouseID", 2, 1)]
        [TestCase("Room_VolumeCubicMetres_Max", 36, 1)]
        [TestCase("Room_VolumeCubicMetres_Sum", 105, 2)]
        [TestCase("RoomSensor_HumidityPercent_Max", 37.1, 2)]
        [TestCase("RoomSensor_TemperatureCelcius_Max", 17, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 1)]
        [TestCase("RoomSensor_TemperatureCelcius_Max", 18, 2)]   
        public void Filter_Grouped_GreaterThanOrEqual_ReturnsExectedCount(string filterColumnUniqueName, object filterValue, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue, FilterModeEnum.GreaterThanOrEqual);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }

        

        [TestCase("Room_Count", 2, 4, 1)]
        [TestCase("Room_Count", 1, 6, 2)]
        [TestCase("RoomSensor_Count", 3, 6, 1)]
        [TestCase("Room_HouseID", 0, 2, 1)]
        [TestCase("Room_HouseID", 0, 3, 2)]
        [TestCase("Room_VolumeCubicMetres_Max", 34, 35, 0)]
        [TestCase("Room_VolumeCubicMetres_Max", 34, 36, 1)]
        [TestCase("Room_VolumeCubicMetres_Sum", 104, 106, 1)]
        [TestCase("RoomSensor_HumidityPercent_Max", 35, 38, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 17, 24, 2)]
        [TestCase("RoomSensor_TemperatureCelcius", 19, 24, 1)]
        [TestCase("RoomSensor_TemperatureCelcius", 17, 20, 1)]   
        public void Filter_Grouped_Between_ReturnsExectedCount(string filterColumnUniqueName, object filterValue1, object filterValue2, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_HouseID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue1, FilterModeEnum.GreaterThan);
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue2, FilterModeEnum.LessThan);

            // act 
            var result = _client.Search(_platform, 1, 1, request);
            var dataTable = result.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            Assert.IsNull(result.Error);
        }


        #endregion








    }
}
