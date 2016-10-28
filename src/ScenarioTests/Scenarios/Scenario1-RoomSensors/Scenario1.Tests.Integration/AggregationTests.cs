using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MagiQL.Framework.Model.Response;
using NUnit.Framework;
using Scenarios.Scenario1.Tests.Integration.Helpers;

namespace Scenarios.Scenario1.Tests.Integration
{
    [TestFixture]
    public class AggregationTests : TestBase
    {
        private const string _maxSuffix = "_Max";
        private const string _minSuffix = "_Min";
        private const string _sumSuffix = "_Sum";
        private const string _averageSuffix = "_Average";
        private const string _bitSuffix = "_Bit";

        [TestCase(_maxSuffix)]
        [TestCase(_minSuffix)]
        [TestCase(_sumSuffix)]
        [TestCase(_averageSuffix)]
        public void AggregateColumns_NoGrouping_MatchesOriginalValue(string aggregationSuffix)
        {
            // arrange
            var searchRequest = SetupRequest(_client, "Room_ID");

            // act
            var result = _client.Search(_platform, 1, 1, searchRequest);

            // assert
            Assert.Greater(result.Data.Count, 0);
            // check all counts = 1
            var allCountColumns = GetAllValuesForColumn(result, c => UniqueName(c).EndsWith("_Count"));
            Assert.IsTrue(allCountColumns.All(x=>x=="1"), "All counts should equal 1 to indiccate no grouping");

            var allAggColumns = _allColumns.Data.Where(x => x.UniqueName.ToLower().EndsWith(aggregationSuffix.ToLower()));
            foreach (var aggCol in allAggColumns)
            {
                Console.Write(aggCol.UniqueName + " VS ");
                var originalCol = _allColumns.Data.First(x => x.UniqueName == aggCol.UniqueName.Replace(aggregationSuffix, ""));
                Console.Write(originalCol.UniqueName + "\n");
                var allAggregateValues = GetAllValuesForColumn(result, c => c.ColumnId == aggCol.Id).ToDecimalString();
                var allOrigValues = GetAllValuesForColumn(result, c => c.ColumnId == originalCol.Id).ToDecimalString();
                AssertListsMatch(allAggregateValues, allOrigValues);
            } 
        }

        [Test]
        public void Max_Grouped_SelectsMaxValue()
        {
            // arrange
            var rawRequest = SetupRequest(_client, "Room_ID");
            var groupedRequest = SetupRequest(_client, "Room_HouseID");

            // act
            var rawResult = _client.Search(_platform, 1, 1, rawRequest);
            var groupedResult = _client.Search(_platform, 1, 1, groupedRequest);

            // assert
            AssertAggregatesCorrectly(rawResult, groupedResult, _maxSuffix, Max);
        }

        [TestCase("Room_HouseID", "1", "Room_ID_Max", 5)]
        [TestCase("Room_HouseID", "2", "Room_ID_Max", 8)]
        [TestCase("Room_HouseID", "1", "Room_VolumeCubicMetres_Max", 35.0)]
        [TestCase("Room_HouseID", "2", "RoomSensor_NoiseDecibels_Max", 29.9)]
        public void Max_Grouped_SingleItemFilter_SelectsMaxValue(string filterColumnUniqueName, string filterValue, string testColumnUniqueName, object expected)
        {
            // arrange 
            var groupedRequest = SetupRequest(_client, "Room_HouseID");
            groupedRequest.AddFilter(_allColumns,filterColumnUniqueName, filterValue);

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, groupedRequest);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(1, groupedResult.Data.Count);
            Assert.AreEqual(expected, dataTable.Rows[0][testColumnUniqueName]);
        }

        [Test]
        public void Min_Grouped_SelectsMinValue()
        {
            // arrange
            var rawRequest = SetupRequest(_client, "Room_ID");
            var groupedRequest = SetupRequest(_client, "Room_HouseID");

            // act
            var rawResult = _client.Search(_platform, 1, 1, rawRequest);
            var groupedResult = _client.Search(_platform, 1, 1, groupedRequest);

            // assert
            AssertAggregatesCorrectly(rawResult, groupedResult, _minSuffix, Min);
        }



        [TestCase("Room_HouseID", "1", "Room_ID", 1)]
        [TestCase("Room_HouseID", "2", "Room_ID", 6)]
        [TestCase("Room_HouseID", "1", "Room_VolumeCubicMetres", 10)]
        [TestCase("Room_HouseID", "2", "RoomSensor_NoiseDecibels", 13.2)]
        public void Min_Grouped_SingleItemFilter_SelectsMinValue(string filterColumnUniqueName, string filterValue, string testColumnUniqueName, object expected)
        {
            // arrange 
            var groupedRequest = SetupRequest(_client, "Room_HouseID");
            groupedRequest.AddFilter(_allColumns, filterColumnUniqueName, filterValue);

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, groupedRequest);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(1, groupedResult.Data.Count);
            Assert.AreEqual(expected, dataTable.Rows[0][testColumnUniqueName]);
        }


        [Test]
        public void Sum_Grouped_SelectsSumValue()
        {
            // arrange
            var rawRequest = SetupRequest(_client, "Room_ID");
            var groupedRequest = SetupRequest(_client, "Room_HouseID");

            // act
            var rawResult = _client.Search(_platform, 1, 1, rawRequest);
            var groupedResult = _client.Search(_platform, 1, 1, groupedRequest);

            // assert
            AssertAggregatesCorrectly(rawResult, groupedResult, _sumSuffix, Sum);
        }

        [TestCase("Room_HouseID", "1", "Room_ID_Sum", (1+2+3+4+5))]
        [TestCase("Room_HouseID", "2", "Room_ID_Sum", (6+7+8))]
        [TestCase("Room_HouseID", "1", "Room_VolumeCubicMetres_Sum", (10 + 25 + 35 + 30 + 25))]
        [TestCase("Room_HouseID", "2", "RoomSensor_NoiseDecibels_Sum", (15.5 + 29.9 + 13.2))]
        public void Sum_Grouped_SingleItemFilter_SelectsSumValue(string filterColumnUniqueName, string filterValue, string testColumnUniqueName, object expected)
        {
            // arrange 
            var groupedRequest = SetupRequest(_client, "Room_HouseID");
            groupedRequest.AddFilter(_allColumns, filterColumnUniqueName, filterValue);

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, groupedRequest);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(1, groupedResult.Data.Count);
            if (dataTable.Rows[0][testColumnUniqueName] is decimal)
            {
                expected = Math.Round(Convert.ToDecimal(expected), 2);
            } 
            Assert.AreEqual(expected, dataTable.Rows[0][testColumnUniqueName]);
        }


        [Test]
        public void Average_Grouped_SelectsAvgValue()
        {
            // arrange
            var rawRequest = SetupRequest(_client, "Room_ID");
            var groupedRequest = SetupRequest(_client, "Room_HouseID");

            // act
            var rawResult = _client.Search(_platform, 1, 1, rawRequest);
            var groupedResult = _client.Search(_platform, 1, 1, groupedRequest);

            // assert
            AssertAggregatesCorrectly(rawResult, groupedResult, _averageSuffix, Average);
        }

        [TestCase("Room_HouseID", "1", "Room_ID_Average", 3)]
        [TestCase("Room_HouseID", "1", "Room_VolumeCubicMetres_Average", (25))]
        [TestCase("Room_HouseID", "2", "RoomSensor_NoiseDecibels_Average", (19.533333))]
        public void Average_Grouped_SingleItemFilter_SelectsAverageValue(string filterColumnUniqueName, string filterValue, string testColumnUniqueName, object expected)
        {
            // arrange 
            var groupedRequest = SetupRequest(_client, "Room_HouseID");
            groupedRequest.AddFilter(_allColumns, filterColumnUniqueName, filterValue);

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, groupedRequest);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(1, groupedResult.Data.Count);
            var actual = dataTable.Rows[0][testColumnUniqueName];
            if (dataTable.Rows[0][testColumnUniqueName] is decimal)
            {
                expected = Math.Round(Convert.ToDecimal(expected), 2);
                actual = Math.Round(Convert.ToDecimal(actual), 2);
            }
            Assert.AreEqual(expected, actual);
        }



        #region Helpers



        private void AssertAggregatesCorrectly(SearchResponse rawResult, SearchResponse groupedResult, string aggregateSuffix, Func<IEnumerable<object>, object> aggregate)
        {
            var columnInfo = _client.GetColumnMappings(_platform, 1, null);
            var rawDt = rawResult.Data.ToDataTable(columnInfo.Data);
            var aggDt = groupedResult.Data.ToDataTable(columnInfo.Data);

            Assert.IsNull(rawResult.Error);
            Assert.Greater(rawResult.Data.Count, 0);
            Assert.IsNull(groupedResult.Error);
            Assert.Greater(groupedResult.Data.Count, 0);
            // check counts 
            var allRawCountColumns = GetAllValuesForColumn(rawResult, c => UniqueName(c).EndsWith("_Count"));
            var allGroupedCountColumns = GetAllValuesForColumn(groupedResult, c => UniqueName(c) == "Room_Count");
            Assert.IsTrue(allRawCountColumns.All(x => x == "1"), "All counts should equal 1 to indiccate no grouping");
            Assert.IsTrue(allGroupedCountColumns.Any(x => int.Parse(x) > 1), "Some counts should be greater than 1 when grouping");


            var allRawGroupKeyColumns = GetAllValuesForColumn(rawResult, c => UniqueName(c) == "Room_HouseID");
            Assert.AreEqual(allRawGroupKeyColumns.Distinct().Count(), groupedResult.Data.Count);


            var allAggColumns = _allColumns.Data.Where(x => x.UniqueName.ToLower().EndsWith(aggregateSuffix.ToLower()));
            foreach (var aggCol in allAggColumns)
            {
                if (aggCol.UniqueName.StartsWith("Room_HouseID")) // cannot aggregte the group key
                {
                    continue;
                }

                Console.Write(aggCol.UniqueName + " VS ");
                var originalCol = _allColumns.Data.First(x => x.UniqueName == aggCol.UniqueName.Replace(aggregateSuffix, ""));
                Console.Write(originalCol.UniqueName + "\n");
                var allOrigValues = GetAllValuesForColumnWithKey(rawDt, originalCol.UniqueName, "Room_HouseID").GroupBy(x => x.Item2);
                var allAggValues = GetAllValuesForColumnWithKey(aggDt, aggCol.UniqueName, "Room_HouseID").GroupBy(x => x.Item2);

                Assert.AreEqual(allAggValues.Count(), allOrigValues.Count());
                foreach (var aggRow in allAggValues)
                {
                    Assert.AreEqual(1, aggRow.Count());
                    var origRow = allOrigValues.FirstOrDefault(x => x.Key.ToString() == aggRow.Key.ToString());

                    var actual = aggRow.Single().Item1;
                    var expected = aggregate(origRow.Select(x => x.Item1));
                    Assert.AreEqual(expected, actual);
                }
            }
        }


        private object Max(IEnumerable<object> values)
        {
            if (values.First() is DateTime)
            {
                return values.Max(x => (DateTime)x);
            }

            if (values.First() is decimal)
            {
                var ds = values.Max(x => (decimal)x);
                return Math.Round(ds, 6);
            }

            if (values.First() is int)
            {
                return values.Max(x => (int)x);
            }

            if (values.First() is Int64)
            {
                return values.Max(x => (Int64)x);
            }
             
            return values.Select(x=>x.ToString()).Max();
        } 

        private object Min(IEnumerable<object> values)
        { 
            if (values.First() is DateTime)
            {
                return values.Min(x => (DateTime)x);
            }

            if (values.First() is decimal)
            {
                var ds = values.Min(x => (decimal)x);
                return Math.Round(ds, 6);
            }

            if (values.First() is int)
            {
                return values.Min(x => (int)x);
            }

            if (values.First() is Int64)
            {
                return values.Min(x => (Int64)x);
            }
             
            return values.Select(x=>x.ToString()).Min();
        }

        private object Sum(IEnumerable<object> values)
        {
            if (values.First() is decimal)
            {
                var ds = values.Sum(x => (decimal)x);
                return Math.Round(ds, 6);
            }

            if (values.First() is int)
            {
                return values.Sum(x => (int)x);
            }

            if (values.First() is Int64)
            {
                return values.Sum(x => (Int64)x);
            }

            throw new NotImplementedException();
        }

        private object Average(IEnumerable<object> values)
        {
            if (values.First() is decimal)
            {
                var ds = values.Average(x => (decimal)x);
                return Math.Round(ds, 6);
            }

            if (values.First() is int)
            {
                return (int)values.Average(x => (int)x); 
            }
            if (values.First() is Int64)
            {
                return (Int64)values.Average(x => (Int64)x); 
            }
             
            throw new NotImplementedException();
        }

        protected List<Tuple<string, string>> GetAllValuesForColumnWithKey(SearchResponse response, Func<ResultColumnValue, bool> columnMatch, string groupKeyColumn)
        {
            var groupColumnId = _allColumns.Data.First(x => x.UniqueName == groupKeyColumn).Id;

            var result =
                response.Data // rows
                .Select(r =>
                    new Tuple<string,string>(
                        r.Values.First(columnMatch).Value, // key
                        r.Values.First(c=>c.ColumnId == groupColumnId).Value // value
                        )
                 )
                .ToList();

            return result;
        }


        private List<Tuple<object, object>> GetAllValuesForColumnWithKey(DataTable dt, string selectColumn, string groupKeyColumn)
        {
            return dt.AsEnumerable()
                .Select(x => new Tuple<object, object>(x[selectColumn], x[groupKeyColumn]))
                .ToList();
        }


        private string UniqueName(ResultColumnValue columnValue)
        {
            return _allColumns.Data.First(x => x.Id == columnValue.ColumnId).UniqueName;
        }

        #endregion
    }
}
