using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Reports.DataAdapters.Base;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;

namespace Scenarios.Scenario3.DataAdapter
{
    public class Scenario3QueryBuilderBase : QueryBuilderBase
    {
        public Scenario3QueryBuilderBase(IDataSourceComponents dataSourceComponents)
            : base(dataSourceComponents)
        {
        }


        public override bool WhereFilterColumnAllowIsNull(MappedSearchRequestFilter filter)
        {
            return true;
        }

        public override bool RequireCurrencyGroupBy(MappedSearchRequest request)
        {
            return false;
        }
    }
}