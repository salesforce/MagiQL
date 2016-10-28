namespace Scenarios.Scenario2.Tests.Integration.Setup
{
    using System.Reflection;

    public class SchemaSetup : ScenarioSetup.SchemaSetup
    {
        public SchemaSetup(Assembly callingAssembly) : base(callingAssembly)
        {
        }

        public override void BuildSchema()
        {
            base.BuildSchema();

            ExecuteScript("Schema", "001.Create-Schema.sql");
            ExecuteScript("Schema", "002.Create-Table-LocationHost.sql");
            ExecuteScript("Schema", "003.Create-Table-Location.sql");
            ExecuteScript("Schema", "004.Create-Table-LocationHit.sql");
        }
    }
}
