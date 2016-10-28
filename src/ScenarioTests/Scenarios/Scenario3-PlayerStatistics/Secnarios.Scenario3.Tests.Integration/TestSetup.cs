using System;
using NUnit.Framework;
using Scenarios.Scenario3.Tests.Integration.Setup;
using ScenarioSetup;
using SchemaSetup = Scenarios.Scenario3.Tests.Integration.Setup.SchemaSetup;

namespace Scenarios.Scenario3.Tests.Integration
{
    [SetUpFixture]
    public class TestSetup
    {
        private IDisposable database;
        private WebApiSetup website;

        [OneTimeSetUp]
        public void DoSetup()
        {
            database = new ScenarioSetup.TestDatabase("MagiQL-Tests", "MagiQL");

            var thisAssembly = typeof(TestSetup).Assembly;

            var schemaSetup = new SchemaSetup(thisAssembly);
            schemaSetup.BuildSchema();

            var dataSetup = new DataSetup(thisAssembly);
            dataSetup.SeedData();

            var columnSetup = new ColumnSetup(thisAssembly);
            columnSetup.CreateColumns();

            website = new WebApiSetup();
            website.Start();

        }

        [OneTimeTearDown]
        public void DoTearDown()
        {
            database.Dispose();
            database = null;

            website.Dispose();
            website = null;
        }
    }
}
