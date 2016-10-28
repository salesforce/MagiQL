using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumns
{
    public abstract partial class CalculatedColumnHelperBase
    {
        public string ReplaceFieldName(Stack<string> stack, string fieldName, string oldName, string newName)
        {
            if (oldName == newName)
            {
                return fieldName;
            }
            if (QueryHelpers.IsCalculatedColumn(newName))
            {
                newName = "(" + ExpandCalculatedColumn(stack, newName) + ")";
            }

            var result = fieldName;
            string pattern = @"\b" + oldName + @"\b";

            result = Regex.Replace(result, pattern, newName);

            return result;
        }

        protected string FixCalculatedFieldNameToUseActualFieldNames(string fieldName, string defaultTable, ref Dictionary<ReportColumnMapping, string> foundColumns, bool useFieldAlias = false)
        {
            var result = fieldName;

            var updatedFoundColumns = foundColumns.ToDictionary(x => x.Key, x => x.Value);

            foreach (var foundColumn in foundColumns)
            {
                string tableAlias = GetTableAlias(foundColumn, defaultTable);

                if (!string.IsNullOrEmpty(tableAlias))
                {
                    tableAlias += ".";
                }

                var foundFieldName = GetFieldName(foundColumn.Key);

                // JMS : i forgot why this is needed - if you work it out please add a comment
                if (Regex.IsMatch(foundFieldName, @"^[0-9.]+$"))
                {
                    tableAlias = null;
                }

                string newFieldName = tableAlias + foundFieldName;

                if (useFieldAlias)
                {
                    newFieldName = GetFieldAlias(foundColumn.Key);
                }

                result = ReplaceFieldName(new Stack<string>(), result, foundColumn.Value, newFieldName);

                var replace = updatedFoundColumns.First(x => x.Value == foundColumn.Value);
                updatedFoundColumns.Remove(replace.Key);
                updatedFoundColumns.Add(replace.Key, newFieldName);
            }

            foundColumns = updatedFoundColumns;

            return result;
        }

        protected string FixCalculatedFieldNameToUseTemporaryFieldNames(string fieldName, string defaultTable, ref Dictionary<ReportColumnMapping, string> foundColumns, out Dictionary<string, string> originalFieldNames, bool useFieldAlias = false)
        {
            var result = fieldName;

            var updatedFoundColumns = foundColumns.ToDictionary(x => x.Key, x => x.Value);
            originalFieldNames = new Dictionary<string, string>();
            int i = 1;
            foreach (var foundColumn in foundColumns)
            {
                string tableAliasOrig = GetTableAlias(foundColumn, defaultTable);
                string tableAlias = "TEMPTBL";

                if (!string.IsNullOrEmpty(tableAlias))
                {
                    tableAliasOrig += ".";
                    tableAlias += ".";
                }

                var foundFieldName = GetFieldName(foundColumn.Key);

                // JMS : i forgot why this is needed - if you work it out please add a comment
                if (Regex.IsMatch(foundFieldName, @"^[0-9.]+$"))
                {
                    tableAliasOrig = null;
                    tableAlias = null;
                }

                string newFieldNameOrig = tableAliasOrig + foundFieldName;
                string newFieldName = tableAlias + "TMPFIELD" + i;

                if (useFieldAlias)
                {
                    newFieldNameOrig = GetFieldAlias(foundColumn.Key);
                }

                result = ReplaceFieldName(new Stack<string>(), result, foundColumn.Value, newFieldName);
                originalFieldNames.Add(newFieldName, newFieldNameOrig); ;

                var replace = updatedFoundColumns.First(x => x.Value == foundColumn.Value);
                updatedFoundColumns.Remove(replace.Key);
                updatedFoundColumns.Add(replace.Key, newFieldName);
                i++;
            }

            foundColumns = updatedFoundColumns;

            return result;
        }

    }
}