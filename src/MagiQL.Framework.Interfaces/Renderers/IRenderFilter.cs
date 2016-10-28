using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Interfaces.Renderers
{
    public interface IRenderFilter
    {
        string ApplyFilter(string value, ReportColumnMapping columnMapping, SearchResultRow row);
    }
}
