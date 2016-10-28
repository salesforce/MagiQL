using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using NUnit.Framework;
using Scenarios.Scenario3.Tests.Integration.Helpers;

namespace Scenarios.Scenario3.Tests.Integration
{
    [TestFixture]
    public class PlayerTests : TestBase
    {
       
        [TestCase("PlayerPhysicalAttributes_HeightCentimetres", true, 185, 9)] // find tallest player
        [TestCase("PlayerAchievements_PrizeMoney", true, 1600, 15)] // find highest earning player
        [TestCase("PlayerAchievements_Count", true, 3, 7)] // find most awarded player
        //[TestCase("PlayerMatchStatistics_Goals", true, , )] // find player with most goals todo
        //[TestCase("PlayerMatchStatistics_DistanceCoveredKilometres", true, , )]// find player with most distance covered todo
        public void GroupedRequest_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            object firstValue, int playerId)
        {
            // arrange 
            var request = SetupRequest(_client, "Player_ID");
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
            Assert.AreEqual(20, dataTable.Rows.Count);

            var firstRow = dataTable.Rows[0][sortColumnUniqueName];
            if (firstRow is decimal)
            {
                firstRow = Math.Round((decimal)firstRow, 7);
            }
            Assert.AreEqual(firstValue, firstRow);
            Assert.AreEqual(playerId, dataTable.Rows[0]["Player_ID"]);
             
        } 


        [TestCase("PlayerPhysicalAttributes_IsMale", "false", "PlayerPhysicalAttributes_WeightKG", false, 57.4, 13)] // find lightest female player  
        public void GroupedRequest_Filtered_SortByTestCase_ReturnsOrderedItems(string filterColumnUniqueName, string filterValue, string sortColumnUniqueName, bool descending, 
            object firstValue, int playerId)
        {
            // arrange 
            var request = SetupRequest(_client, "Player_ID");
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue);
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

            var firstRow = dataTable.Rows[0][sortColumnUniqueName];
            if (firstRow is decimal)
            {
                firstRow = Math.Round((decimal)firstRow, 7);
            }
            Assert.AreEqual(firstValue, firstRow);
            Assert.AreEqual(playerId, dataTable.Rows[0]["Player_ID"]);

        } 

         
        [TestCase("Player_Count", true, 15, true)] // how many players are male  
        [TestCase("Player_Count", false, 5, false)] // how many players are female
        public void GroupedRequest_SummarizeByGender_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
            object firstValue, bool isMale)
        {
            // arrange 
            var request = SetupRequest(_client, "Player_ID");
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SummarizeByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == "PlayerPhysicalAttributes_IsMale").Id);
            request.SortDescending = descending;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows[0][sortColumnUniqueName] is decimal)
            {
                firstValue = Math.Round(Convert.ToDecimal(firstValue), 7);
            }
            Assert.AreEqual(2, dataTable.Rows.Count);

            var firstRow = dataTable.Rows[0][sortColumnUniqueName];
            if (firstRow is decimal)
            {
                firstRow = Math.Round((decimal)firstRow, 7);
            }
            Assert.AreEqual(firstValue, firstRow);
            Assert.AreEqual(isMale, dataTable.Rows[0]["PlayerPhysicalAttributes_IsMale"]);

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

 