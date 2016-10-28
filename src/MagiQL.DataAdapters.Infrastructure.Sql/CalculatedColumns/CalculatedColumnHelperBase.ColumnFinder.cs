using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumns
{
    public abstract partial class CalculatedColumnHelperBase
    {
        public List<string> FindColumnNamesInCalculatedField(string fieldName)
        {
            const string pattern = @"[\[a-zA-z@\]][^ ()+\-*\\/%,]+"; // a-z without punctuation
            var regex = new Regex(pattern);
            var matches = regex.Matches(fieldName);

            var result = matches.Cast<Match>().Select(match => match.Value).Distinct().ToList();

            result = result.Where(x =>
                !SqlStopWords.Contains(x)
                && x != "_C"
                && !x.StartsWith("dbo.")
                && !x.StartsWith("@")).ToList();

            return result;
        }

        public Dictionary<ReportColumnMapping, string> FindColumnsInCalculatedField(string fieldName, string table = null, bool throwOnNoMatch = true)
        {
            const string aliasPattern = "^c([^ ]*)(_[0-9]+){1,2}$"; // c(UniqueName)_### or c(UniqueName)_###_###

            var result = new Dictionary<ReportColumnMapping, string>();

            //var foundNames = FindColumnNamesInCalculatedField(fieldName);
            var foundNames = FindColumnNamesInCalculatedFieldWithAggregationMethod(fieldName);
            foreach (var fieldWithAlias in foundNames)
            {
                ReportColumnMapping matchingColumn;

                // table.field format
                if (fieldWithAlias.Item1.Contains('.'))
                {
                    var tableName = fieldWithAlias.Item1.Split('.')[0].TrimStart('[').TrimEnd(']');
                    if (tableName == "dbo")
                    {
                        continue;
                    }

                    var field = fieldWithAlias.Item1.Split('.')[1];

                    var aggregationMethod = fieldWithAlias.Item2 ?? FieldAggregationMethod.Exclude; // todo : get this from the query
                    matchingColumn = FindColumnByFieldName(tableName, field, aggregationMethod);
                }
                // match on alias format (including action stats format)
                else if (Regex.IsMatch(fieldWithAlias.Item1, aliasPattern))
                {
                    // todo : refactor as a method into subclasses
                    var uniqueName = Regex.Match(fieldWithAlias.Item1, aliasPattern).Groups[1].Value;
                    uniqueName = Regex.Replace(uniqueName, "_[0-9]+$", ""); // for action stats 
                    matchingColumn = FindColumnByUniqueName(uniqueName);
                }
                else
                {
                    matchingColumn = FindColumnByUniqueName(fieldWithAlias.Item1);
                }

                if (matchingColumn != null)
                {
                    if (!result.ContainsKey(matchingColumn))
                    {
                        result.Add(matchingColumn, fieldWithAlias.Item1);
                    }
                }
                else if (throwOnNoMatch)
                {
                    throw new Exception("Could not find column for " + fieldWithAlias + " ( in calculated column formula '" + fieldName + "' )");
                }
            }

            return result;
        }
        
        public List<Tuple<string, FieldAggregationMethod?>> FindColumnNamesInCalculatedFieldWithAggregationMethod(string fieldName)
        {
            const string pattern = @"(MIN\(|MAX\(|AVG\(|SUM\()*[\[a-zA-z@\]][^ ()+\-*\\/%,]+"; // a-z without punctuation or Min(field, MAX(field, AVG(field, SUM(field
            var regex = new Regex(pattern);
            var matches = regex.Matches(fieldName);

            var distinctMatches = matches.Cast<Match>().Select(match => match.Value).Distinct().ToList();

            var result = new List<Tuple<string, FieldAggregationMethod?>>();

            foreach (var r in distinctMatches)
            {
                var key = r;
                string agg = null;
                FieldAggregationMethod? aggregate = null;

                if (r.Contains("("))
                {
                    agg = r.Split('(')[0];
                    key = r.Split('(')[1];
                }

                if (SqlStopWords.Contains(key)
                    || key == "_C"
                    || key.StartsWith("dbo.")
                    || key.StartsWith("@"))
                {
                    continue;
                }

                if (agg != null)
                {
                    switch (agg.ToLower())
                    {
                        case "min": aggregate = FieldAggregationMethod.Min;
                            break;
                        case "max": aggregate = FieldAggregationMethod.Max;
                            break;
                        case "sum": aggregate = FieldAggregationMethod.Sum;
                            break;
                        case "avg": aggregate = FieldAggregationMethod.Average;
                            break;
                    }
                }
                result.Add(new Tuple<string, FieldAggregationMethod?>(key, aggregate));
            }


            return result;
        }
        
        internal ReportColumnMapping FindColumnByUniqueName(string uniqueName)
        {
            var found = ColumnProvider.Find(DataSourceId, uniqueName);
            if (found != null && found.Count > 1)
            {
                throw new Exception("More than one column found matching '" + uniqueName + "'");
            }

            return found != null ? found.SingleOrDefault() : null;
        }

        protected abstract ReportColumnMapping FindColumnByFieldName(string table, string field, FieldAggregationMethod aggregationMethod);

//        protected virtual ReportColumnMapping FindColumnByFieldName(string table, string field, FieldAggregationMethod aggregationMethod)
//        {
//            var found = ColumnProvider.Find(DataSourceId, table, field, null);
//
//            if (found.Count > 1)
//            {
//                throw new Exception(string.Format("Found more than one column for {0}.{1}", table, field));
//            }
//
//            return found.SingleOrDefault();
//        }
    }
}