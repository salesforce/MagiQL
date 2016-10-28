using ScenarioSetup;

namespace Scenarios.Scenario3.Tests.Integration.Setup
{
    using System.Reflection;

    public class DataSetup : ScriptRunner
    {
        public DataSetup(Assembly callingAssembly) : base(callingAssembly)
        {
        }

        public void SeedData()
        {
            ExecuteScript("Data", "001.Seed-Team.sql");
            ExecuteScript("Data", "002.Seed-Player.sql");
            ExecuteScript("Data", "003.Seed-PlayerPhysicalAttributes.sql");
            ExecuteScript("Data", "004.Seed-PlayerAchievements.sql");
            ExecuteScript("Data", "005.Seed-Match.sql");
            ExecuteScript("Data", "006.Seed-PlayerMatchStatistics.sql");
        }

    }
}