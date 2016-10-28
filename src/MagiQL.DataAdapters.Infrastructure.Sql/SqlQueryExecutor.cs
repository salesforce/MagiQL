using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Dapper;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Framework.Model.Response.Base;
using SqlModeller.Compiler.Model;
using SqlModeller.Shorthand;

namespace MagiQL.DataAdapters.Infrastructure.Sql
{
    public class SqlQueryExecutor : ISqlQueryExecutor
    { 
        private int sqlCommandTimeoutSeconds
        {
            get
            {
                if (ConfigurationManager.AppSettings["SqlCommandTimeoutSeconds"] != null)
                {
                    int result = 30;
                    if (int.TryParse(ConfigurationManager.AppSettings["SqlCommandTimeoutSeconds"], out result))
                    {
                        return result;
                    }
                }
                return 30;
            }
        } 

        public SearchResult Search(IReportsDataSource dataSource, SearchRequest request, bool doNotExecute = false)
        { 
            var result = new SearchResult();
            var searchResultSummary = new SearchResultSummary();
             
            var query = new SqlModeller.Model.Query();
            CompiledQuery compiledQuery = null;
            string sql = null;

            if (request.SelectedColumns == null || !request.SelectedColumns.Any())
            {
                throw new Exception("No Columns Selected");
            }
            try
            {
                using (new QuickTimer(x => searchResultSummary.BuildQueryElapsedMilliseconds = x))
                {
                    long mapTime;
                    query = dataSource.BuildQuery(request, out mapTime);
                    searchResultSummary.MapRequestElapsedMilliseconds = mapTime;
                }

                long countResult = 0;

                using (new QuickTimer(x => searchResultSummary.CompileQueryElapsedMilliseconds = x))
                {
                    compiledQuery = query.Compile();
                }

                // add OPTION (RECOMPILE) to combat parameter sniffing slowness
                sql = compiledQuery.Sql + "\n OPTION (RECOMPILE) ";

                DynamicParameters sqlParams = null;
                if (compiledQuery.Parameters != null)
                {
                    sqlParams = new DynamicParameters();
                    foreach (var p in compiledQuery.Parameters)
                    {
                        sqlParams.Add(p.ParameterName, p.Value, p.DataType);
                    }
                }

                if (!doNotExecute)
                {  
                    using (var connection = ConnectionFactory.GetOpenConnectionUsingConnectionStringName(dataSource.ConnectionStringName))
                    {
                        Debug.WriteLine(compiledQuery.ToString());
                        IEnumerable<dynamic> queryResult;

                        using (new QuickTimer(x => searchResultSummary.QueryElapsedMilliseconds = x))
                        {
                            using (var tran = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                            {
                                queryResult = connection.Query(sql, sqlParams, tran, commandTimeout: sqlCommandTimeoutSeconds);
                                tran.Commit();
                            }
                        }

                        using (new QuickTimer(x => searchResultSummary.ParseResultElapsedMilliseconds = x))
                        {
                            result = ParseQueryResult(dataSource, queryResult, request, out countResult);
                        }
                    }
                }

                searchResultSummary.TotalRows = countResult;
                result.Summary = searchResultSummary;

            }
            catch (Exception ex)
            {
                result = new SearchResponse()
                {
                    Error = new ResponseError().Load(ex)
                };
            }

            try
            {
                result.DebugInfo.SqlQuery = compiledQuery.ParameterSql + "\n\n" + sql;
            }
            catch
            {
            }

            return result;
        }
         
        private SearchResponse ParseQueryResult(IReportsDataSource dataSource, IEnumerable<dynamic> queryResult, SearchRequest request, out long countResult)
        {
            var result = new SearchResponse();
            result.Data = new List<SearchResultRow>();
            countResult = 0;

            int rowIndex = 1;
            var allColumnMappings = dataSource.GetColumnMappings(request.SelectedColumns);
            // need to preserve order
            var selectedColumnMappings = request.SelectedColumns.Select(x => allColumnMappings.First(y => y.Id == x.ColumnId)).ToList();
            
            foreach (dynamic doc in queryResult)
            {
                var data = new SearchResultRow();
                data.Values = new List<ResultColumnValue>();

                IDictionary<string, object> propertyValues = (IDictionary<string, object>)doc;

                foreach (var col in selectedColumnMappings)
                {
                    var columnAlias = dataSource.GetFieldAlias(col);
                    var displayName = dataSource.GetColumnDisplayName(col);

                    object value = null;

                    if (!propertyValues.ContainsKey(columnAlias))
                    {
                        result.DebugInfo.WarningMessages.Add(
                            String.Format(
                                "Cannot find the '{0}' column (column alias: '{1}') in the database query result set. It could be a column that's not allowed for this type of stats query. For example, the DateTime column can only be requested for stats queries that return timestamped data (e.g. queries with a daily temporal aggregation). Will set this value to null in the response data.",
                                displayName, columnAlias));
                    }
                    else
                    {
                        value = propertyValues[columnAlias];
                    }

                    data.Values.Add(new ResultColumnValue
                    {
                        ColumnId = col.Id,
                        Value = value == null ? null : value.ToString(),
                        Name = (request.DebugMode || rowIndex == 1) ? displayName : null
                    });
                }

                result.Data.Add(data);

                if (rowIndex == 1 && propertyValues.ContainsKey("_COUNT"))
                {
                    countResult = long.Parse(propertyValues["_COUNT"].ToString());
                }

                rowIndex ++;
            }

            return result;
        }
    }
}
