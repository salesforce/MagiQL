using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Interfaces.Renderers;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Services
{
    public class RenderFilterService : IRenderFilterService
    {
        private readonly IRenderFilterFactory _renderFilterFactory;

        public RenderFilterService(IRenderFilterFactory renderFilterFactory)
        {
            _renderFilterFactory = renderFilterFactory;
        }

        public void ApplyAllRenderFilters(IReportsDataSource dataSource, SearchResult searchResult)
        {
            if (searchResult != null && searchResult.Data != null && searchResult.Data.Any())
            {
                var allFilters = _renderFilterFactory.GetFilters();
                var selectedColumnIds = searchResult.Data.First().Values.Select(x => x.ColumnId);
                var allColumnMappings = dataSource.GetColumnProvider().GetColumnMappings(dataSource.DataSourceId, selectedColumnIds);

                foreach (var row in searchResult.Data)
                {
                    foreach (var cell in row.Values)
                    {
                        var mapping = allColumnMappings.FirstOrDefault(x => x.Id == cell.ColumnId);
                        if (mapping != null)
                        {
                            ApplyRenderFilters(allFilters, cell, mapping);
                        }
                    }
                }
            }
        }

        private void ApplyRenderFilters(List<IRenderFilter> allFilters, ResultColumnValue cell, ReportColumnMapping mapping)
        {
            foreach (var renderFilter in allFilters)
            {
                cell.Value = renderFilter.ApplyFilter(cell.Value, mapping, null);
            }
        }
    }
}