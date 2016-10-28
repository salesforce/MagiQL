using System;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Scenarios.Scenario3.Tests.Integration
{
    [TestFixture]
    public class QueryTests : TestBase
   {
        [Test]
        public void GetColumns_ReturnsColumns()
        {
            // arrange

            // act
            var result = _client.GetSelectableColumns(_platform);

            // assert
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            Assert.Greater(result.Data.Count, 0);
        }

        [Test]
        public void GetColumnMappings_ReturnsColumns()
        {
            // arrange

            // act
            var result = _client.GetColumnMappings(_platform,1,null,clearCache:true);

            // assert
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            Assert.Greater(result.Data.Count, 0);
        }

        [Test]
        public void Query_ReturnsRows()
        { 
            // arrange
            var searchRequest = SetupRequest(_client, "Team_ID");

            // act
            var result = _client.Search(_platform, 1, 1, searchRequest);
             
            // assert
            Console.WriteLine(JsonConvert.SerializeObject(result.Data, Formatting.Indented));
            Assert.Greater(result.Data.Count, 0);
        }


    }

}
