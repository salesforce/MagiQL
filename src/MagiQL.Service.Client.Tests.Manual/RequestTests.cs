using System.Threading.Tasks;

namespace MagiQL.Service.Client.Tests.Manual
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework.Model;
    using Framework.Model.Request;
    using NUnit.Framework;
    using System.Threading.Tasks;
    [TestFixture]
    public class RequestTests
    {
        private string platform = "facebook";

        [Test]
        public void GetColumns()
        {
            var client = new ReportsServiceClient();

            var columns = client.GetSelectableColumns(this.platform, 1, null, null);
             

            Assert.IsNotNull(columns);
            Assert.IsNotNull(columns.Data);
            Assert.GreaterOrEqual(columns.Data.Count, 1);

            Console.WriteLine("{0} columns returned", columns.Data.Count);

        }

        [Test]
        public async Task GetColumnsAsync()
        {
            var client = new ReportsServiceClient();

            client.RequestHeaders = new List<RequestHeader>
            {
                new RequestHeader() {Name = "x-correlation-id", Value = Guid.NewGuid().ToString()},
                new RequestHeader() {Name = "x-requesting-component", Value = "Tests"}
            };

            var columns = await client.GetSelectableColumnsAsync(this.platform, 1, null, null);

            Assert.IsNotNull(columns);
            Assert.IsNotNull(columns.Data);
            Assert.GreaterOrEqual(columns.Data.Count, 1);

            Console.WriteLine("{0} columns returned", columns.Data.Count); 
        }


        [Test]
        public void SimpleQuery()
        {
            var client = new ReportsServiceClient();

            var columns = client.GetSelectableColumns(this.platform, 1, null, null);

            var request = new SearchRequest()
            {
                SelectedColumns = columns.Data.Take(30).Select(x=>new SelectedColumn(x.Id)).ToList(),
                TemporalAggregation = TemporalAggregation.Total,
                PageSize = 100,
                SortByColumn = columns.Data.Where(x=>x.UniqueName=="CampaignID").Select(x => new SelectedColumn(x.Id)).First(),
                GroupByColumn = columns.Data.Where(x=>x.UniqueName=="CampaignID").Select(x => new SelectedColumn(x.Id)).First(),
                GetCount = true,
            };

            client.RequestHeaders = new List<RequestHeader>
            {
                new RequestHeader() {Name = "x-correlation-id", Value = Guid.NewGuid().ToString()},
                new RequestHeader() {Name = "x-requesting-component", Value = "Tests"}
            };

            var result = client.Search(this.platform, 1, null, request); 

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.GreaterOrEqual(result.Data.Count, 1);

            Console.WriteLine("{0} rows returned", result.Data.Count); 

        }


        [Test]
        public async Task SimpleQueryAsync()
        {
            var client = new ReportsServiceClient();

            var columns = await client.GetSelectableColumnsAsync(this.platform, 1, null, null);

            var request = new SearchRequest()
            {
                SelectedColumns = columns.Data.Take(30).Select(x => new SelectedColumn(x.Id)).ToList(),
                TemporalAggregation = TemporalAggregation.Total,
                PageSize = 100,
                SortByColumn = columns.Data.Where(x => x.UniqueName == "CampaignID").Select(x => new SelectedColumn(x.Id)).First(),
                GroupByColumn = columns.Data.Where(x => x.UniqueName == "CampaignID").Select(x => new SelectedColumn(x.Id)).First(),
                GetCount = true,
            };


            var result = await client.SearchAsync(this.platform, 1, null, request);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.GreaterOrEqual(result.Data.Count, 1);

            Console.WriteLine("{0} rows returned", result.Data.Count);

        }

        [Test]
        public void SimpleQuery_GroupByCurrencyCode()
        {
            var client = new ReportsServiceClient();

            var columns = client.GetSelectableColumns(this.platform, 1, null, null);

            var request = new SearchRequest()
            {
                SelectedColumns = columns.Data.Take(30).Select(x => new SelectedColumn(x.Id)).ToList(),
                TemporalAggregation = TemporalAggregation.Total,
                PageSize = 100,
                SortByColumn = columns.Data.Where(x => x.UniqueName == "Campaign_CurrencyCode").Select(x => new SelectedColumn(x.Id)).First(),
                GroupByColumn = columns.Data.Where(x => x.UniqueName == "Campaign_CurrencyCode").Select(x => new SelectedColumn(x.Id)).First(),
                GetCount = true,
                Filters = new List<SearchRequestFilter>() { new SearchRequestFilter()
                    {
                        ColumnId = columns.Data.First(x=>x.UniqueName.EndsWith("UIStatus")).Id,
                        Values = new List<string>{"1"}
                    } 
                }
            };

         
            var result = client.Search(this.platform, 1, null, request);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.GreaterOrEqual(result.Data.Count, 1);
            Assert.LessOrEqual(result.Data.Count, 5); // assume we wont ever have > 5 currencies

            Console.WriteLine("{0} rows returned", result.Data.Count);

            

        }



    }
}
