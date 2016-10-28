namespace ScenarioSetup
{
    using System.Reflection;

    public class SchemaSetup : ScriptRunner
    {
        public SchemaSetup(Assembly callingAssembly) : base(callingAssembly)
        {
        }

        public virtual void BuildSchema()
        {
            var assembly = typeof(SchemaSetup).Assembly;

            ExecuteScript("Schema", "001.Create-Table-ReportColumnMapping.sql", assembly);
            ExecuteScript("Schema", "002.Create-Table-ReportColumnMappingMetaData.sql", assembly);
            ExecuteScript("Schema", "003.Create-Table-ReportStatus.sql", assembly);
        }
    }
}
