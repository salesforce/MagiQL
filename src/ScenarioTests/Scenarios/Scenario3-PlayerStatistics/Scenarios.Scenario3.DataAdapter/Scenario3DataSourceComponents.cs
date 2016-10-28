using MagiQL.Framework.Interfaces;
using MagiQL.Reports.DataAdapters.Base;

namespace Scenarios.Scenario3.DataAdapter
{
    public class Scenario3DataSourceComponents : DataSourceComponentsBase
    {
        public Scenario3DataSourceComponents(IColumnProvider columnProvider) : base(columnProvider)
        {
        }

        protected override void RegisterComponents()
        {
            Constants = new Constants();
            TableMappings = new Scenario3TableMappings();

            QueryBuilderBase = new Scenario3QueryBuilderBase(this);
        }

    }
}
