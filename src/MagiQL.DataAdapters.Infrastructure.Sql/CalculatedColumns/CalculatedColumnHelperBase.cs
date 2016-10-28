using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumnCompiler;
using MagiQL.DataAdapters.Infrastructure.Sql.Functions;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;
using SqlModeller.Compiler.QueryParameterManagers;
using SqlModeller.Model;
using SqlModeller.Model.Select;

namespace MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumns
{
    public abstract partial class CalculatedColumnHelperBase
    {
        protected readonly IColumnProvider ColumnProvider;
        protected readonly int DataSourceId;

        public CalculatedColumnHelperBase(IColumnProvider columnProvider, int dataSourceId)
        {
            ColumnProvider = columnProvider;
            DataSourceId = dataSourceId;
        }

        protected static List<string> SqlStopWords = new List<string> { "CASE", "WHEN", "THEN", "ELSE", "END", "AS", "AND", "OR", "NOT", "NULL", "COUNT", "MIN", "MAX", "AVG", "SUM", "NULLIF", "DATEPART", "DATEDIFF", "DATEADD", "HOUR", "DAY", "CONCAT", "CEILING", "FLOOR", "IS", "ISNULL" }
            .Union(FunctionRegistry.All.Select(x => x.Name))
            .Distinct().ToList();

      
        public abstract string GetFieldAlias(ReportColumnMapping column);

        protected abstract string GetTableAlias(KeyValuePair<ReportColumnMapping, string> foundColumn, string defaultTable);

        protected abstract string GetStatsTable(bool useDateStats);
         
        protected virtual string GetFieldName(ReportColumnMapping column)
        {
            return column.FieldName;
        }
         
        public string GetCalculatedColumnFieldName(string fieldName, bool useDateStats, bool dontAggregate = false, bool useFieldAlias = false)
        {
            using (new DebugTimer("CalculatedColumnHelper.GetCalculatedColumnFieldName"))
            {
                string originalFieldName = fieldName;
                fieldName = originalFieldName;

                if (!QueryHelpers.IsCalculatedColumnCompiled(fieldName))
                {
                    fieldName = new SqlExpressionParser().ConvertToSql(fieldName);
                }

                fieldName = ExpandCalculatedColumn(new Stack<string>(), fieldName, useFieldAlias);

                var columnsInCalculation = FindColumnsInCalculatedField(fieldName);
                var statsTable = GetStatsTable(useDateStats);
                //fieldName = FixCalculatedFieldNameToUseActualFieldNames(fieldName, statsTable, ref columnsInCalculation, useFieldAlias);
                Dictionary<string, string> tempFieldNameLookups;
                fieldName = FixCalculatedFieldNameToUseTemporaryFieldNames(fieldName, statsTable, ref columnsInCalculation, out tempFieldNameLookups, useFieldAlias);

                if (!QueryHelpers.IsCalculatedColumnCompiled(fieldName))
                {
                    fieldName = new SqlExpressionParser().ConvertToSql(fieldName);
                }

                var shouldAggregate = !dontAggregate && fieldName.Contains(' ');


                foreach (var foundColumn in columnsInCalculation)
                {
                    var aggregate = QueryHelpers.GetAggregate(foundColumn.Key.FieldAggregationMethod);
                    string replaceColumnName = tempFieldNameLookups[foundColumn.Value];
                    if (shouldAggregate)
                    {
                        replaceColumnName = GenerateAggregateColumnSql(aggregate, replaceColumnName);
                    }
                    fieldName = ReplaceFieldName(new Stack<string>(), fieldName, foundColumn.Value, replaceColumnName);
                }

                return fieldName;
            }
        }

        private static string GenerateAggregateColumnSql(Aggregate aggregate, string columnName)
        {
            var compiler = new SqlModeller.Compiler.SqlServer.SelectCompilers.ColumnSelectorCompiler();
            var result = compiler.Compile(new ColumnSelector(null, columnName, "REMOVEME", aggregate), new SelectQuery(), new NoQueryParameterManager());
            result = result.Replace(" AS REMOVEME", "");
            return result;
            //return string.Format("{0}({1})", aggregate.ToString().ToUpper(), columnName);
        }
        

    }
}
