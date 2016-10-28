using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace MagiQL.DataAdapters.Infrastructure.Sql
{
    public static class ConnectionFactory
    {
        public static IDbConnection GetOpenConnection(string connectionString)
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var cnn = factory.CreateConnection();
            cnn.ConnectionString = connectionString;
            cnn.Open();

            return cnn;
        }

        public static IDbConnection GetOpenConnectionUsingConnectionStringName(string connectionStringName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionString == null)
            {
                throw new Exception(string.Format("No ConnectionString found with name '{0}'",connectionStringName));
            }

            return GetOpenConnection(connectionString.ConnectionString);
        }
    }
}