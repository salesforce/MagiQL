using System.Collections.Generic;
using System.Data;
using System.Linq;
using MagiQL.Framework.Model.Request;
using NUnit.Framework;
using Scenarios.Scenario3.Tests.Integration.Helpers;

namespace Scenarios.Scenario3.Tests.Integration
{
    [TestFixture]
    public class TextSearchTests : TestBase
    {
        [TestCase("Ch", 4)]
        [TestCase("Bryan Bourne", 1)]
        public void TextSearch_SimpleText_ReturnsMatchingItems(string query, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Player_ID");
            request.TextFilter = query;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(expectedCount, groupedResult.Data.Count);
            foreach (DataRow row in dataTable.Rows)
            {
                Assert.True(row["Player_Name"].ToString().ToLower().Contains(query.ToLower()));
            }
        }

        [TestCase("Bryan Bourne", 0)]
        [TestCase("\"Team A\"", 5)]
        [TestCase("Team", 20)]
        public void TextSearch_SimpleTextWithTextFilterColumn_ReturnsMatchingItems(string query, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Player_ID");
            request.TextFilter = query;

            int textFilterColumnId = _allColumns.Data.First(x => x.UniqueName.Equals("Team_Name")).Id;
            request.TextFilterColumns = new List<SelectedColumn>()
            {
                new SelectedColumn() {ColumnId = textFilterColumnId}
            };

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(expectedCount, groupedResult.Data.Count);
            foreach (DataRow row in dataTable.Rows)
            {
                Assert.True(row["Team_Name"].ToString().ToLower().Contains(query.ToLower().Trim('\"')));
            }
        }

        [TestCase("Bryan Bourne", 1)]
        [TestCase("\"Team A\"", 5)]
        [TestCase("Team", 20)]
        public void TextSearch_SimpleTextWithTwoTextFilterColumn_ReturnsMatchingItems(string query, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Player_ID");
            request.TextFilter = query;

            int textFilterColumnId = _allColumns.Data.First(x => x.UniqueName.Equals("Team_Name")).Id;
            int textFilterColumnId2 = _allColumns.Data.First(x => x.UniqueName.Equals("Player_Name")).Id;
            request.TextFilterColumns = new List<SelectedColumn>()
            {
                new SelectedColumn() { ColumnId = textFilterColumnId },
                new SelectedColumn() { ColumnId = textFilterColumnId2 }
            };

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(expectedCount, groupedResult.Data.Count);
            foreach (DataRow row in dataTable.Rows)
            {
                Assert.True(row["Team_Name"].ToString().ToLower().Contains(query.ToLower().Trim('\"')) ||
                    row["Player_Name"].ToString().ToLower().Contains(query.ToLower().Trim('\"')));
            }
        }
    }
}
