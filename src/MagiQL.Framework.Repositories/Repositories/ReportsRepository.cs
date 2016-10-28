using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using DapperExtensions;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Repositories.Dapper;

namespace MagiQL.Framework.Repositories.Repositories
{
    public abstract class ReportsRepository<T>
        where T : class
    {

        private const string _connectionStringName = "MagiQL";

        protected ReportsRepository()
        {
            MapperRegistry.Initialize();
        }

        internal IDbConnection Connection
        {
            get
            { 
                var connectionStringConfig = ConfigurationManager.ConnectionStrings[_connectionStringName];
                if (connectionStringConfig != null)
                {
                    return new SqlConnection(connectionStringConfig.ConnectionString);
                }
                throw new Exception(string.Format("ConnectionString with name '{0}' not found", _connectionStringName));
            }
        }

        public virtual IDbTransaction CreateTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            var connection = Connection;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            return connection.BeginTransaction(isolationLevel);
        }
        
        protected IDbConnection GetConnection(IDbTransaction scope)
        {
            if (scope.Connection.State == ConnectionState.Closed)
            {
                scope.Connection.Open();
            }
            return scope.Connection;
        }

        public virtual IEnumerable<T> All()
        {
            IEnumerable<T> items = null;

            using (IDbConnection cn = Connection)
            {
                cn.Open();
                items = cn.GetList<T>();
            }

            return items;
        }

        public virtual IEnumerable<T> QueryWhere(string whereClause, object parameters)
        {
            var sql = string.Format("SELECT * FROM {0} WHERE {1}", typeof (T).Name, whereClause);

            return Connection.Query<T>(sql, parameters);
        }

        public virtual T Get(object id)
        {
            return Connection.Get<T>(id);
        }

        public virtual T Add(T entity, IDbTransaction scope)
        {
            GetConnection(scope).Insert(entity, scope);
            return entity;
        }

        public virtual void Update(ReportColumnMapping entity, IDbTransaction scope)
        {
            GetConnection(scope).Update(entity, scope);
        }
         
        public virtual void Delete(T entity, IDbTransaction scope)
        {
            GetConnection(scope).Delete(entity, scope);
        }

    }
}