using MagiQL.Expressions.Model;

namespace MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumnCompiler
{
    public class SqlEvaluator
    { 
        public string Evaluate(Expression expression)
        {
            return new SqlEvaluatorVisitor().Visit(expression) as string;
        } 
    }
}