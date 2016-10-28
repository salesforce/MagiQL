using System;
using System.Data;
using System.Linq;
using MagiQL.Framework.Model.Request;
using NUnit.Framework;
using Scenarios.Scenario1.Tests.Integration.Helpers;

namespace Scenarios.Scenario1.Tests.Integration
{
    [TestFixture]
    public class CalculatedColumnTests : TestBase
    {
        private SortByTests sortTests;

        [SetUp]
        public void SetUp()
        {
            sortTests = new SortByTests();
            sortTests.Setup();
        }

        [TestCase("Calculated_HumidityPercentTimesTemperatureCelciusMin", true, (39.10 * 26), (28.90 * 18))]
        [TestCase("Calculated_HumidityPercentTimesTemperatureCelciusMin", false, (28.90 * 18), (39.10 * 26))]
        [TestCase("Calculated_IsLightCount", true, 1, 0)]
        [TestCase("Calculated_IsLightCount", false, 0, 1)]
        [TestCase("Calculated_TemperaturePerCubicMetre", true, (20.0 / 10.0), (20.0 / 45.0))]
        [TestCase("Calculated_TemperaturePerCubicMetre", false, (20.0 / 45.0), (20.0 / 10.0))]
        public void UngroupedRequest_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            object firstValue, object lastValue)
        {
           sortTests.UngroupedRequest_SortByTestCase_ReturnsOrderedItems(sortColumnUniqueName,descending,firstValue,lastValue);   
        }

        [TestCase("Calculated_HumidityPercentTimesTemperatureCelciusMin", true, (33.70 * 20), (28.80 * 18))]
        [TestCase("Calculated_HumidityPercentTimesTemperatureCelciusMin", false, (28.80 * 18), (33.70 * 20))]
        [TestCase("Calculated_IsLightCount", true, 0, 0)]
        [TestCase("Calculated_IsLightCount", false, 0, 0)]
        [TestCase("Calculated_TemperatureVariance", false, 6, 6)]
        //[TestCase("Calculated_TemperaturePerCubicMetre", true, (68 / 105.0), (102 / 125.0))] // todo : bug, always using min, not sum
        //[TestCase("Calculated_TemperaturePerCubicMetre", false, (102 / 125.0), (68 / 105.0))] // todo : bug, always using min, not sum
        public void GroupedRequest_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            object firstValue, object lastValue)
        {
           sortTests.GroupedRequest_SortByTestCase_ReturnsOrderedItems(sortColumnUniqueName,descending,firstValue,lastValue);   
        }
        
        [TestCase("Calculated_HumidityPercentTimesTemperatureCelciusMin", true, (33.70 * 20), (28.80 * 18))]
        [TestCase("Calculated_HumidityPercentTimesTemperatureCelciusMin", false, (28.80 * 18), (33.70 * 20))]
        [TestCase("Calculated_IsLightCount", true, 3, 1)]
        [TestCase("Calculated_IsLightCount", false, 1, 3)]
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

        [TestCase("Calculated_HumidityPercentTimesTemperatureCelciusMin", 547.20, 1)]
        [TestCase("Calculated_TemperaturePerCubicMetre", 0.6333333, 1)]
        //[TestCase("Calculated_TemperatureVariance", 6, 2)] bug, aggregation is happening before the sum
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
                filterValue = Math.Round(Convert.ToDecimal(filterValue),7);
            }

            Assert.IsNull(result.Error);
            Assert.AreEqual(expectedCount, dataTable.Rows.Count);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var actual = dataRow[filterColumnUniqueName];
                if (actual is decimal)
                {
                    actual = Math.Round((decimal)actual, 7);
                }

                Assert.AreEqual(filterValue, actual);
            }
        }

    }
}
