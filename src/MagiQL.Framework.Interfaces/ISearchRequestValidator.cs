using MagiQL.Framework.Model.Request;

namespace MagiQL.Framework.Interfaces
{
    public interface ISearchRequestValidator
    {
        void Validate(string platform, int? organizationId, SearchRequest request);
    }
}