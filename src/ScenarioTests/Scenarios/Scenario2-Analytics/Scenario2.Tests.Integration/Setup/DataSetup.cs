using ScenarioSetup;

namespace Scenarios.Scenario2.Tests.Integration.Setup
{
    using System.Reflection;

    public class DataSetup : ScriptRunner
    {
        public DataSetup(Assembly callingAssembly) : base(callingAssembly)
        {
        }

        public void SeedData()
        {
            ExecuteScript("Data", "001.Seed-LocationHost.sql");
            ExecuteScript("Data", "002.Seed-Location.sql");
            ExecuteScript("Data", "003.Seed-LocationHit.sql");
        }

    }
}