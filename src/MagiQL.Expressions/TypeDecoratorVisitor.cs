using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class TypeDecoratorVisitor<T> : Visitor
	{
		private SymbolRegistry<T> Symbols { get; set; }
		public bool ThrowOnError { get; private set; }
		
		public TypeDecoratorVisitor(SymbolRegistry<T> symbols, bool throwOnError)
		{
			Symbols = symbols;
			ThrowOnError = throwOnError;
		}

		public override object Visit(IdentifierExpression ex)
		{
			var name = ex.Identifier.ToLower();

			var dataType = Symbols.FindType(name);

			if (dataType != null)
			{
				ex.DataType = (DataType)dataType;
			}
			else
			{
				if (ThrowOnError)
				{
					throw new ExpressionException("Unrecognized identifier '" + ex.Identifier + "'");
				}
			}

			return null;
		}

		public override object Visit(BinaryExpression ex)
		{
			ex.Left.Visit(this);
			ex.Right.Visit(this);

			return null;
		}

		public override object Visit(UnaryExpression ex)
		{
			ex.Expression.Visit(this);

			return null;
		}
	}

	public class TypeDecoratorVisitor : TypeDecoratorVisitor<object>
	{
		public TypeDecoratorVisitor(SymbolRegistry symbols) : base(symbols, true)
		{
			
		}
	}
}
