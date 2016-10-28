using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces.Services
{
    public interface IRenderFilterService
    { 
        void ApplyAllRenderFilters(IReportsDataSource dataSource, SearchResult searchResult);
    }
}