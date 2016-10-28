using System.Collections.Generic;
using System.Linq;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Validation
{
    public static class SqlInjectionChecker
    {
        private static List<string> SqlInjectionStrings = new List<string>
        {
            "--",
            ";--",
            ";",
            "/*",
            "*/",
            "@@",
            "@",
            "char",
            "nchar",
            "varchar",
            "nvarchar",
            "alter",
            "cast",
            "cursor",
            "declare",
            "drop",
            "exec",
            "execute",
            "fetch",
            "insert",
            "kill",
            "select",
            "sys",
            "sysobjects",
            "syscolumns",
            "table",

            // need a space to avoid column name matches
            "create ",
            "delete ",
            "update "
        };
      

        public static bool HasInjection(string fieldName)
        {
            string fieldNameLowered = fieldName.ToLower();
            return SqlInjectionStrings.Any(x=> fieldNameLowered.Contains(x));
        }
    }
}
