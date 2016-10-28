using System;
using System.Data.SqlClient;
using System.Data.SqlLocalDb;
using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace ScenarioSetup
{ 
    /// <summary>
    /// A temporary test database that gets automatically deleted when disposed.
    /// </summary>
    public class TestDatabase : IDisposable
    {
        private string databaseName;
        private string instanceName; 
        private string connectionString;
        private string mdfFilePath;
        private string ldfFilePath;
        private ISqlLocalDbInstance instance;

        public TestDatabase(string instanceName, string databaseName)
        {
            this.Initialize(instanceName, databaseName);
        }

        public string ConnectionString {get { return this.connectionString; }}

        public string DatabaseName {get { return this.databaseName;}}
        public string InstanceName { get { return this.instanceName; } }

        public void Dispose()
        {
//            if (this.instance != null)
//            {
//                  instance.Stop();
                    // delete
//            }
//
//            if (File.Exists(this.mdfFilePath))
//            {
//                File.Delete(this.mdfFilePath);
//            }
//
//            if (File.Exists(this.ldfFilePath))
//            {
//                File.Delete(this.ldfFilePath);
//            }
        }

        private void Initialize(string instanceName, string databaseName)
        {
            this.databaseName = databaseName;
            this.instanceName = instanceName;

            var existing = SqlLocalDbApi.GetInstanceInfo(instanceName);
            if (existing.Exists)
            {
                if (existing.IsRunning)
                {
                    SqlLocalDbApi.StopInstance(instanceName);
                }
                SqlLocalDbApi.DeleteInstance(instanceName);
            }


            ISqlLocalDbProvider provider = new SqlLocalDbProvider();
            this.instance = provider.GetOrCreateInstance(instanceName);

            instance.Start();


            var connectionStringBuilder = instance.CreateConnectionStringBuilder();

            using (var conn = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                var serverConnection = new ServerConnection(conn);

                // By default, LocalDB stores database files in the user's home folder. Messy for temporary test databases.
                // Store them in the user's temp folder instead. 
                var testDatabasesDirectory = Path.Combine(Path.GetTempPath(), "TestDatabases");

                if (!Directory.Exists(testDatabasesDirectory))
                {
                    Directory.CreateDirectory(testDatabasesDirectory);
                }

                // Generate a unique name for our mdf file to avoid clashing with other ongoing test runs.
                var databaseFileNameRoot = string.Format("{0}_{1}_{2}", databaseName, DateTime.Now.ToString("yyyyMMdd_HHmmss"), Guid.NewGuid().ToString("N"));

                var mdfFileName = databaseFileNameRoot + ".mdf";
                var ldfFileName = databaseFileNameRoot + "_log.ldf";

                this.mdfFilePath = Path.Combine(testDatabasesDirectory, mdfFileName);
                this.ldfFilePath = Path.Combine(testDatabasesDirectory, ldfFileName);

                var sql = string.Format("CREATE DATABASE {0} ON (NAME = N'{0}', FILENAME = '{1}')", databaseName, this.mdfFilePath);

                Console.WriteLine(string.Format("Creating database {0} at {1}", databaseName, this.mdfFilePath));
                serverConnection.ExecuteNonQuery(sql);
            }

            connectionStringBuilder.InitialCatalog = this.databaseName;
            this.connectionString = connectionStringBuilder.ConnectionString;
        }
    } 
}
