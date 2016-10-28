using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class Evaluator<T>
	{
		private T Data { get; set; }
		private SymbolRegistry<T> SymbolRegistry { get; set; }

		
		public Evaluator(SymbolRegistry<T> symbols)
		{
			SymbolRegistry = symbols;
		}

		public Evaluator(SymbolRegistry<T> symbols, T data)
		{
			Data = data;
			SymbolRegistry = symbols;
		}

		public object Evaluate(Expression expression)
		{
			return new EvaluatorVisitor<T>(SymbolRegistry, Data).Visit(expression);
		}

		public object Evaluate(Expression expression, T data)
		{
			var result = new EvaluatorVisitor<T>(SymbolRegistry, data).Visit(expression);
		    return result;
		}
	}

	public class Evaluator : Evaluator<object>
	{
		public Evaluator()
			: base(null)
		{

		}
	}
}
