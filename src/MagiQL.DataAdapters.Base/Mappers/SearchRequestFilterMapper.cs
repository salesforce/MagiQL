using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Request;

namespace MagiQL.Reports.DataAdapters.Base.Mappers
{
    public class SearchRequestFilterMapper
    {
        private readonly ConstantsBase _constants;

        public IColumnProvider ColumnProvider { get; set; }

        public SearchRequestFilterMapper(IColumnProvider columnProvider, ConstantsBase constants)
        {
            _constants = constants;
            ColumnProvider = columnProvider;
        }

        public List<MappedSearchRequestFilter> Map(List<SearchRequestFilter> filters)
        {
            if (filters == null)
            {
                return new List<MappedSearchRequestFilter>();
            }

            return filters.Select(Map).ToList();
        }

        public MappedSearchRequestFilter Map(SearchRequestFilter filter)
        {
            var result = new MappedSearchRequestFilter
            {
                Exclude = filter.Exclude,
                Mode = filter.Mode,
                ProcessBeforeAggregation = filter.ProcessBeforeAggregation,
                Values = filter.Values,
                Column = ColumnProvider.GetColumnMapping(_constants.DataSourceId, filter.ColumnId.Value)
            };
            return result;
        }
    }
}