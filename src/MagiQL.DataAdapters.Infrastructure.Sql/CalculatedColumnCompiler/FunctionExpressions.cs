using System.Collections.Generic;
using MagiQL.DataAdapters.Infrastructure.Sql.Functions;

namespace MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumnCompiler
{
    public class FunctionExpressions
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public string OriginalText { get; set; }
        public string ParsedText { get; set; }

        public List<string> OriginalArgumentExpressions { get; set; } 
        public List<string> ParsedArgumentExpressions { get; set; }

        public InlineSqlFunction Function { get; set; }
    }
}