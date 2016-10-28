using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Dapper;

namespace ScenarioSetup
{
    public abstract class ScriptRunner
    {
        private readonly Assembly callingAssembly;

        public ScriptRunner(Assembly callingAssembly)
        {
            this.callingAssembly = callingAssembly;
        }

        public void ExecuteScript(string folderName, string fileName, Assembly assembly = null)
        {
            Console.WriteLine("Execute {0}/{1}", folderName, fileName);

            var sourceAssembly = assembly ?? this.callingAssembly;
            var resourceName = sourceAssembly.FullName.Split(',')[0] + ".SqlScripts" + "." + folderName + "." + fileName;
            string sql = GetEmbeddedResourceContent(sourceAssembly, resourceName);
            ConnectAndExecute(x => SqlMapper.Execute(x, sql));
        }

        public string GetEmbeddedResourceContent(Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        private void ConnectAndExecute(Action<SqlConnection> action)
        {
            string connectionString = "Server=(localdb)\\MagiQL-Tests;Initial Catalog=MagiQL;Integrated Security=true;";
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                action.Invoke(sqlConnection);
                sqlConnection.Close();
            }
        }

    }
}