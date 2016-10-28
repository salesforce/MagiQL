using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumnCompiler;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumns
{
    public abstract partial class CalculatedColumnHelperBase
    {
        /// <summary>
        /// Finds nested calculated columns in a fieldname and replaces the referenced calculated column with the full formula
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string ExpandCalculatedColumn(Stack<string> existingFieldNames, string fieldName, bool useFieldAlias = false)
        {
            var result = fieldName;
            int recursions = 3;

            while (recursions > 0) // needed for synonym columns
            {
                recursions--;

                var foundColumns = FindColumnsInCalculatedField(result, throwOnNoMatch: false);
                if (!foundColumns.Any(x => x.Key.IsCalculated))
                {
                    break;
                }

                var calculatedColumns = new List<KeyValuePair<ReportColumnMapping, string>>();

                // either the fieldname has a formula
                // or the field name is a reference to another field
                foreach (var found in foundColumns)
                {
                    if (found.Key.IsCalculated)
                    {
                        if (existingFieldNames.Contains(fieldName))
                        {
                            throw new ArgumentException("Recursion detected on field " + fieldName, "fieldName");
                        }
                        existingFieldNames.Push(fieldName);

                        string newFieldName = found.Key.FieldName;

                        if (useFieldAlias && found.Key.FieldAggregationMethod != FieldAggregationMethod.Average)
                        {
                            newFieldName = GetFieldAlias(found.Key);
                        }
                        else
                        {
                            var nestedColumns = FindColumnsInCalculatedField(found.Value, throwOnNoMatch: false);
                            if (nestedColumns.Any())
                            {
                                if (nestedColumns.Count > 1)
                                {
                                    calculatedColumns.Add(found);
                                }
                                else if (nestedColumns.First().Key.IsCalculated)
                                {
                                    calculatedColumns.Add(found);
                                }
                            }
                        }

                        if (QueryHelpers.IsCalculatedColumn(newFieldName) && !QueryHelpers.IsCalculatedColumnCompiled(newFieldName))
                        {
                            newFieldName = new SqlExpressionParser().ConvertToSql(newFieldName);
                        }

                        result = ReplaceFieldName(existingFieldNames, result, found.Value, newFieldName);
                        existingFieldNames.Pop();

                    }
                }
            }

            return result;
        }

    }
}