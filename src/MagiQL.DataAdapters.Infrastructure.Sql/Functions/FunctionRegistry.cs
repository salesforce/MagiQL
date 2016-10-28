using System.Collections.Generic;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class FunctionRegistry
    { 
        public static List<InlineSqlFunction> All = new List<InlineSqlFunction>
        {  
            new MaxOfFunction(),
            new MinOfFunction(), 
            new CeilingFunction(),
            new FloorFunction(),
            new IfThenElseFunction(),
            new ToBoolFunction(), 
            new ToBigIntFunction(), 
            new IsNullFunction(),
            new NullIfFunction(), 
            new CountFunction(),
            new RoundDateFunction()
        };
    }
}
