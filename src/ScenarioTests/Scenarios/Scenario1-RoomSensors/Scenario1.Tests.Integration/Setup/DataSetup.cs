using ScenarioSetup;

namespace Scenarios.Scenario1.Tests.Integration.Setup
{
    using System.Reflection;

    public class DataSetup : ScriptRunner
    {
        public DataSetup(Assembly callingAssembly) : base(callingAssembly)
        {
        }

        public void SeedData()
        {
            ExecuteScript("Data", "001.Seed-Room.sql");
            ExecuteScript("Data", "002.Seed-RoomSensor.sql");
        }

    }
}