using System.Data;
using NUnit.Framework;
using Scenarios.Scenario1.Tests.Integration.Helpers;

namespace Scenarios.Scenario1.Tests.Integration
{
    [TestFixture]
    public class TextSearchTests : TestBase
    {
        [TestCase("Bedroom", 3)]
        [TestCase("Bedroom 1", 2)]
        [TestCase("hallway", 1)] // ignore case
        [TestCase("hall", 1)]  // partial
        [TestCase("way", 1)]  // partial
        public void TextSearch_SimpleText_ReturnsMatchingItems(string query, int expectedCount)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.TextFilter = query;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(expectedCount, groupedResult.Data.Count);
            foreach (DataRow row in dataTable.Rows)
            {
                Assert.True(row["Room_Name"].ToString().ToLower().Contains(query.ToLower()));
            }
        }


        [TestCase("Bedroom -1", "Bedroom 2")]
        [TestCase("Bedroom 2", "Bedroom 2")]
        [TestCase("Bed room 2", "Bedroom 2")]
        [TestCase("Bedroom +2", "Bedroom 2")]
        [TestCase("2 +room Bedroom", "Bedroom 2")]
        public void TextSearch_FreeText_ReturnsSingleMatchingItem(string query, string expectedName)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.TextFilter = query;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);
            var dataTable = groupedResult.Data.ToDataTable(_allColumnInfo.Data);

            // assert
            Assert.AreEqual(1, groupedResult.Data.Count);
            Assert.AreEqual(expectedName, dataTable.Rows[0]["Room_Name"]);
        }


        [TestCase("Bedroom -1 -2")] 
        [TestCase("Bedroom -bed")] 
        [TestCase("Garden")] 
        public void TextSearch_FreeText_ReturnsNoItem(string query)
        {
            // arrange 
            var request = SetupRequest(_client, "Room_ID");
            request.TextFilter = query;

            // act 
            var groupedResult = _client.Search(_platform, 1, 1, request);

            // assert
            Assert.AreEqual(0, groupedResult.Data.Count); 
        }
    }
}
