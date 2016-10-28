using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Reports.DataAdapters.Base;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;

namespace Scenarios.Scenario1.DataAdapter
{
    public class Scenario1QueryBuilderBase : QueryBuilderBase
    {
        public Scenario1QueryBuilderBase(IDataSourceComponents dataSourceComponents)
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