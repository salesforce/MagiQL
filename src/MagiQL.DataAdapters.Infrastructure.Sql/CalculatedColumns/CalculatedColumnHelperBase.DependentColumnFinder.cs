//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Text.RegularExpressions;
//using MagiQL.DataAdapters.Infrastructure.Sql.Functions;
//using MagiQL.Framework.Interfaces;
//using MagiQL.Framework.Model.Columns;
//using SqlModeller.Compiler.QueryParameterManagers;
//using SqlModeller.Model;
//using SqlModeller.Model.Select;

//namespace MagiQL.DataAdapters.Infrastructure.Sql
//{
//    public abstract partial class CalculatedColumnHelperBase
//    {
//        public List<ReportColumnMapping> FindDependentColumns(ReportColumnMapping calculatedColumn)
//        {
//            var result = new List<ReportColumnMapping>();
//            FindDependentColumnsImpl(new Stack<string>(), result, calculatedColumn);
//            return result;
//        }

//        private void FindDependentColumnsImpl(Stack<string> existing, List<ReportColumnMapping> results,
//            ReportColumnMapping current)
//        {
//            if (existing.Contains(current.UniqueName))
//            {
//                throw new Exception("Circular reference");
//            }

//            if (!results.Contains(current))
//            {
//                results.Add(current);
//            }

//            if (!current.IsCalculated)
//            {
//                return;
//            }

//            existing.Push(current.UniqueName);
//            var referencedColumns = FindColumnNamesInCalculatedField(current.FieldName).Select(FindColumnByUniqueName);
//            foreach (var reportColumnMapping in referencedColumns)
//            {
//                FindDependentColumnsImpl(existing, results, reportColumnMapping);
//            }
//            existing.Pop();
//        }


//    }
//}