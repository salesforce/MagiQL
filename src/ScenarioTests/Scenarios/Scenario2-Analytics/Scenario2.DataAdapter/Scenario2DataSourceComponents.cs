using MagiQL.Framework.Interfaces;
using MagiQL.Reports.DataAdapters.Base;

namespace Scenarios.Scenario2.DataAdapter
{
    public class Scenario2DataSourceComponents : DataSourceComponentsBase
    {
        public Scenario2DataSourceComponents(IColumnProvider columnProvider) : base(columnProvider)
        {
        }

        protected override void RegisterComponents()
        {
            Constants = new Constants();
            TableMappings = new Scenario2TableMappings();

            QueryBuilderBase = new Scenario2QueryBuilderBase(this); 
        }

    }

   
}
