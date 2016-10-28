namespace MagiQL.Service.Client.Tests.Manual
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Framework.Model;
    using Framework.Model.Request;
    using NUnit.Framework;

    [TestFixture]
    public class SimultaneousRequestTests
    {
        private string platform = "facebook";

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        public void SimpleQuery_MultiThreaded(int threadCount)
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

            Parallel.For(0, threadCount, x =>
            {
                var result = client.Search(this.platform, 1, null, request);

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Data);
                Assert.GreaterOrEqual(result.Data.Count, 1);

                Console.WriteLine("{0} rows returned", result.Data.Count);                

            }); 

        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        public void SimpleQuery_GroupByCurrencyCode_MultiThreaded(int threadCount)
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

            Parallel.For(0, threadCount, x =>
            {
                var result = client.Search(this.platform, 1, null, request);

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Data);
                Assert.GreaterOrEqual(result.Data.Count, 1);
                Assert.LessOrEqual(result.Data.Count, 5); // assume we wont ever have > 5 currencies

                Console.WriteLine("{0} rows returned", result.Data.Count);

            });

        }  
        
        
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        public void SimpleQuery_Mixed_MultiThreaded(int threadCount)
        {
            var client = new ReportsServiceClient();

            var columns = client.GetSelectableColumns(this.platform, 1, null, null);

            var request1 = new SearchRequest()
            {
                SelectedColumns = columns.Data.Take(30).Select(x => new SelectedColumn(x.Id)).ToList(),
                TemporalAggregation = TemporalAggregation.Total,
                PageSize = 100,
                SortByColumn = columns.Data.Where(x => x.UniqueName == "CampaignID").Select(x => new SelectedColumn(x.Id)).First(),
                GroupByColumn = columns.Data.Where(x => x.UniqueName == "CampaignID").Select(x => new SelectedColumn(x.Id)).First(),
                GetCount = true,
            };

            var request2 = new SearchRequest()
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

            Parallel.For(0, threadCount, x =>
            {
                var result1 = client.Search(this.platform, 1, null, request1);
                Assert.IsNotNull(result1);
                Assert.IsNotNull(result1.Data);
                Assert.GreaterOrEqual(result1.Data.Count, 1);
                Console.WriteLine("{0} rows returned", result1.Data.Count);


                var result2 = client.Search(this.platform, 1, null, request2);
                Assert.IsNotNull(result2);
                Assert.IsNotNull(result2.Data);
                Assert.GreaterOrEqual(result2.Data.Count, 1);
                Console.WriteLine("{0} rows returned", result2.Data.Count); 
                Assert.LessOrEqual(result2.Data.Count, 5); // assume we wont ever have > 5 currencies


            });

        }



    }
}
