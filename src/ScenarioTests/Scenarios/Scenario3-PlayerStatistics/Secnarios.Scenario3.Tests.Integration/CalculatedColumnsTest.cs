using System;
using System.Linq;
using MagiQL.Framework.Model.Request;
using NUnit.Framework;
using Scenarios.Scenario3.Tests.Integration.Helpers;

namespace Scenarios.Scenario3.Tests.Integration
{
    [TestFixture]
    public class CalculatedColumnsTest : TestBase
    { 
        // PLAYER
         
        // player goals / game
        // player minutes / goal
        // distance per tackle 
       

        [TestCase("Player_ID", "6", "PlayerMatchStatistics_Count", 5)] // Games Played (count)
        [TestCase("Player_ID", "7", "PlayerMatchStatistics_Count", 1)] // Games Played (count)
        [TestCase("Player_ID", "18", "Calculated_PlayerStatsManOfTheMatchCount", 3)]  // Man of the match count
        [TestCase("Player_ID", "7", "Calculated_PlayerStatsMinutesPlayed", 34)]   // Minutes played  
        [TestCase("Player_ID", "10", "Calculated_PlayerStatsMinutesPlayed", 111)]  // Minutes played  
        [TestCase("Player_ID", "11", "Calculated_PlayerStatsSubsOff", 2)]  // Substitutions off 
        [TestCase("Player_ID", "18", "Calculated_PlayerStatsSubsOff", 0)]  // Substitutions off 
       
        public void PlayerStatsSummaryRequest_Filtered_ReturnsExpectedValue(string filterColumnUniqueName, string filterValue, string sortColumnUniqueName, object firstValue)
        {
            // arrange 
            var request = SetupRequest(_client, "PlayerMatchStatistics_ID");
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SummarizeByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == "Player_ID").Id); 
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue);


            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            Assert.IsNull(groupedResult.Error);
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

        } 


        // MATCH

        [TestCase("PlayerMatchStatistics_MatchID", "1", "Calculated_MatchHomeGoals", 1)] // home team goals
        [TestCase("PlayerMatchStatistics_MatchID", "1", "Calculated_MatchAwayGoals", 0)] // away team goals
        [TestCase("PlayerMatchStatistics_MatchID", "2", "Calculated_MatchHomeGoals", 3)]// home team goals
        [TestCase("PlayerMatchStatistics_MatchID", "2", "Calculated_MatchAwayGoals", 3)]// away team goals
        public void MatchSummaryRequest_Filtered_ReturnsExpectedValue(string filterColumnUniqueName, string filterValue, string sortColumnUniqueName, object firstValue)
        {
            // arrange 
            var request = SetupRequest(_client, "PlayerMatchStatistics_ID");
            request.SortByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == sortColumnUniqueName).Id);
            request.SummarizeByColumn = new SelectedColumn(_allColumns.Data.First(x => x.UniqueName == "PlayerMatchStatistics_MatchID").Id);
            request.AddFilter(_allColumns, filterColumnUniqueName, filterValue);
             

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            Assert.IsNull(groupedResult.Error);
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

        } 
        
    }
}
