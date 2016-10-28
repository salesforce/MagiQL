namespace Scenarios.Scenario3.Tests.Integration.Setup
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
            ExecuteScript("Schema", "002.Create-Table-Team.sql");
            ExecuteScript("Schema", "003.Create-Table-Player.sql");
            ExecuteScript("Schema", "004.Create-Table-PlayerPhysicalAttributes.sql");
            ExecuteScript("Schema", "005.Create-Table-PlayerAchievements.sql");
            ExecuteScript("Schema", "006.Create-Table-Match.sql");
            ExecuteScript("Schema", "007.Create-Table-PlayerMatchStatistics.sql");
        }
    }
}
