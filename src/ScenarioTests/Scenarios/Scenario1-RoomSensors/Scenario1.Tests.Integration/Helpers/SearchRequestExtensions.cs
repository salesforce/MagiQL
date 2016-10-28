using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;

namespace Scenarios.Scenario1.Tests.Integration.Helpers
{
    public static class SearchRequestExtensions
    {
        public static SearchRequest AddFilter(this SearchRequest request, GetSelectableColumnsResponse allColumns, string uniqueColumnName, object value, FilterModeEnum mode = FilterModeEnum.Equal)
        {
            if (request.Filters == null)
            {
                request.Filters = new List<SearchRequestFilter>();
            }

            var column = allColumns.Data.Where(x=>x.UniqueName == uniqueColumnName).Select(x => new SelectedColumn(x.Id)).FirstOrDefault();

            request.Filters.Add(new SearchRequestFilter()
            {
                ColumnId = column.ColumnId,
                Mode = mode,
                Values = new List<string>() { value.ToString() }
            });

            return request;
        }
    }
}
