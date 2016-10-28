using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces
{
    public interface ISqlQueryExecutor
    { 
        SearchResult Search(IReportsDataSource dataSource, SearchRequest request, bool doNotExecute = false);
    }
}