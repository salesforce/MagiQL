using MagiQL.Framework.Interfaces;
using MagiQL.Reports.DataAdapters.Base;

namespace Scenarios.Scenario1.DataAdapter
{
    public class Scenario1DataSourceComponents : DataSourceComponentsBase
    {
        public Scenario1DataSourceComponents(IColumnProvider columnProvider) : base(columnProvider)
        {
        }

        protected override void RegisterComponents()
        {
            Constants = new Constants();
            TableMappings = new Scenario1TableMappings();

            QueryBuilderBase = new Scenario1QueryBuilderBase(this);
        }

    }
}
