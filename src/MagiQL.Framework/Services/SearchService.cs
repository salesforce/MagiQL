using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Services
{
    /// <summary>
    /// Retrieves the correct datasource, executes requests and applies render filters before returning the result
    /// </summary>
    public class SearchService : ISearchService
    {
        private readonly IReportsDataSourceFactory reportsDataSourceFactory;
        private readonly ISqlQueryExecutor sqlQueryExecutor;
        private readonly IRenderFilterService renderFilterService;

        public SearchService(
            IReportsDataSourceFactory reportsDataSourceFactory, 
            ISqlQueryExecutor sqlQueryExecutor,
            IRenderFilterService renderFilterService)
        {
            this.reportsDataSourceFactory = reportsDataSourceFactory;
            this.sqlQueryExecutor = sqlQueryExecutor;
            this.renderFilterService = renderFilterService;
        }

        public SearchResult Search(string platform, SearchRequest request, bool doNotExecute)
        {
            var dataSource = reportsDataSourceFactory.GetDataSource(platform);
            var searchResult = sqlQueryExecutor.Search(dataSource, request, doNotExecute);

            renderFilterService.ApplyAllRenderFilters(dataSource, searchResult);

            return searchResult;
        }

    }
}
