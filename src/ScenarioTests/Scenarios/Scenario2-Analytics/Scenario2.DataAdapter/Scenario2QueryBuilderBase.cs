using MagiQL.DataAdapters.Infrastructure.Sql.Model;
using MagiQL.Reports.DataAdapters.Base;
using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Base;

namespace Scenarios.Scenario2.DataAdapter
{
    public class Scenario2QueryBuilderBase : QueryBuilderBase
    {
        public Scenario2QueryBuilderBase(IDataSourceComponents dataSourceComponents)
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