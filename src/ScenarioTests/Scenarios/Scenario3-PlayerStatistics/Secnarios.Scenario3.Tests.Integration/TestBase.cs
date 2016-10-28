using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Service.Client;
using NUnit.Framework;

namespace Scenarios.Scenario3.Tests.Integration
{
    public abstract class TestBase
    {
        protected const string _platform = "Scenario3";

        protected ReportsServiceClient _client;

        protected GetSelectableColumnsResponse _allColumns;

        protected GetColumnMappingsResponse _allColumnInfo;


        [SetUp]
        public void Setup()
        {
            _client = new ReportsServiceClient();
            _allColumnInfo = _client.GetColumnMappings(_platform, 1, null, clearCache:true);
            _allColumns = _client.GetSelectableColumns(_platform, null, null); 
        }

        protected SearchRequest SetupRequest(ReportsServiceClient client, string groupByColumnUniqueName, string sortByColumnUniqueName = null)
        {
            var selected = _allColumns.Data.Select(x => new SelectedColumn(x.Id)).ToList();
            var groupBy = _allColumns.Data.Single(x => x.UniqueName.ToLower() == groupByColumnUniqueName.ToLower());
            var sortBy = _allColumns.Data.Single(x => x.UniqueName.ToLower() == (sortByColumnUniqueName ?? groupByColumnUniqueName).ToLower());
            
            var pageIndex = 0;
            var searchRequest = new SearchRequest()
            {
                SelectedColumns = selected,
                SortByColumn = new SelectedColumn(sortBy.Id),
                GroupByColumn = new SelectedColumn(groupBy.Id),
                DebugMode = true,
                GetCount = pageIndex == 0,
                PageIndex = pageIndex,
                PageSize = 50,
                SortDescending = false,
            };

            return searchRequest;
        }

        protected void AssertListsMatch(IEnumerable<string> left, IEnumerable<string> right)
        {
            Assert.AreEqual(string.Join(",", left), string.Join(",", right));
        }
          

        protected List<string> GetAllValuesForColumn(SearchResponse response, Func<ResultColumnValue, bool> columnMatch)
        {
            var result =
                response.Data // rows
                .SelectMany(r =>
                    r.Values // columns
                        .Where(columnMatch).Select(x => x.Value)
                ).ToList();

            return result;
        }
    
    
    }
}
