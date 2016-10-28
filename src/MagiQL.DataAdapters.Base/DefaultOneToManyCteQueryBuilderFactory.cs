using MagiQL.Reports.DataAdapters.Base.DataSource.QueryExecutor.QueryBuilders.Data;

namespace MagiQL.Reports.DataAdapters.Base
{
    public class DefaultOneToManyCteQueryBuilderFactory
    {
        protected IDataSourceComponents _dataSourceComponents;

        public DefaultOneToManyCteQueryBuilderFactory(IDataSourceComponents dataSourceComponents)
        {
            _dataSourceComponents = dataSourceComponents;
        }

        public virtual DefaultOneToManyCteQueryBuilder Create(string knownTableName)
        {
            return new DefaultOneToManyCteQueryBuilder(_dataSourceComponents, knownTableName);
        }
    }
}