using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;

namespace Scenarios.Scenario2.Tests.Integration.Helpers
{
    public static class SearchResultExtensions
    {
        public static DataTable ToDataTable(this List<SearchResultRow> rows, List<ReportColumnMapping> columnInfo)
        {
            var dataTable = new DataTable();

            if (rows!=null && rows.Any())
            {
                foreach (var column in rows.First().Values)
                {
                    var col = columnInfo.First(x => x.Id == column.ColumnId);
                    dataTable.Columns.Add(col.UniqueName, col.DbType.ToClrType());
                }

                foreach (var row in rows)
                {
                    var values = GetRowValues(row, columnInfo);
                    dataTable.Rows.Add(values);

                }
            }

            return dataTable;
        }

        private static object[] GetRowValues(SearchResultRow row, List<ReportColumnMapping> columnInfo)
        {
            var list = new List<object>();
            foreach (var column in row.Values)
            {
                var col = columnInfo.First(x => x.Id == column.ColumnId);
                object obj = null;
                try
                {
                    obj = Convert.ChangeType(column.Value, col.DbType.ToClrType());
                }
                catch{}
                list.Add(obj);
            }
            return list.ToArray();
        }
    }
}
