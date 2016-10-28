using System;
using System.Linq;
using MagiQL.Framework.Model.Request;
using NUnit.Framework;
using Scenarios.Scenario3.Tests.Integration.Helpers;

namespace Scenarios.Scenario3.Tests.Integration
{
    [TestFixture]
    public class TeamTests : TestBase
    {
        // group by Team_ID
        //-------------------
          
        [TestCase("PlayerMatchStatistics_Goals", true, 10, 4)] // find team with most goals scored
        [TestCase("PlayerMatchStatistics_Goals", false, 1, 2)] // find team with least goals scored
        [TestCase("PlayerMatchStatistics_DistanceCoveredKilometres", true, 50.8, 4)] // find team with most distance covered
        [TestCase("PlayerMatchStatistics_DistanceCoveredKilometres", false, 42.9, 1)] // find team with least  distance covered
        public void GroupedRequest_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
         object firstValue, int teamId)
        {
            // arrange 
            var request = SetupRequest(_client, "Team_ID");
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
            Assert.AreEqual(4, dataTable.Rows.Count);

            var firstRow = dataTable.Rows[0][sortColumnUniqueName];
            if (firstRow is decimal)
            {
                firstRow = Math.Round((decimal)firstRow, 7);
            }
            Assert.AreEqual(firstValue, firstRow);
            Assert.AreEqual(teamId, dataTable.Rows[0]["Team_ID"]);

        }


        [TestCase("Calculated_PlayerStatYellowCardCount", true, 4, 3)] // find team with most yellow cards
        [TestCase("Calculated_PlayerStatYellowCardCount", false, 0, 2)] // find team with least yellow cards
         public void GroupedRequest_SummarizedByTeam_SortByTestCase_ReturnsOrderedItems(string sortColumnUniqueName, bool descending,
         object firstValue, int teamId)
        {
            // arrange 
            var request = SetupRequest(_client, "PlayerMatchStatistics_ID");
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SummarizeByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == "Team_ID").Id);
            request.SortDescending = descending;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert 
            if (dataTable.Rows[0][sortColumnUniqueName] is decimal)
            {
                firstValue = Math.Round(Convert.ToDecimal(firstValue), 7);
            }
            Assert.AreEqual(4, dataTable.Rows.Count);

            var firstRow = dataTable.Rows[0][sortColumnUniqueName];
            if (firstRow is decimal)
            {
                firstRow = Math.Round((decimal)firstRow, 7);
            }
            Assert.AreEqual(firstValue, firstRow);
            Assert.AreEqual(teamId, dataTable.Rows[0]["Team_ID"]);

        } 
        

    }
}
