using System.Collections.Generic;

namespace MagiQL.Framework.Interfaces.Renderers
{
    public interface IRenderFilterFactory
    {
        List<IRenderFilter> GetFilters();
    }
}