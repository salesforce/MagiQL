using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces.Services
{
    public interface ISearchService
    {
        SearchResult Search(string platform, SearchRequest request, bool doNotExecute = false);
    }
}