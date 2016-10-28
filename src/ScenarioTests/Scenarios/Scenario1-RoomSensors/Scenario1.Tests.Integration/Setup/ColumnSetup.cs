namespace Scenarios.Scenario1.Tests.Integration.Setup
{
    using System.Reflection;

    public class ColumnSetup : ScenarioSetup.ScriptRunner
    {
        public ColumnSetup(Assembly callingAssembly) : base(callingAssembly)
        {
        }

        public void CreateColumns()
        {
            ExecuteScript("Columns", "001.DeleteColumnMappings.sql");
            ExecuteScript("Columns", "002.AutoInsertColumnMappings.sql");
            ExecuteScript("Columns", "003.AutoInsertColumnMappings_Avg.sql");
            ExecuteScript("Columns", "004.AutoInsertColumnMappings_Count.sql");
            ExecuteScript("Columns", "005.AutoInsertColumnMappings_Max.sql");
            //ExecuteScript("Columns", "006.AutoInsertColumnMappings_Min.sql");
            ExecuteScript("Columns", "007.AutoInsertColumnMappings_Sum.sql");
            ExecuteScript("Columns", "008.InsertCalculatedColumns.sql");
        }
    }
}
