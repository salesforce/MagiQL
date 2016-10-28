using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class TreeInterpreterVisitor : TreeInterpreterVisitor<object>
	{
		public TreeInterpreterVisitor() : base(null)
		{

		}
	}

	public class TreeInterpreterVisitor<T> : Visitor
	{
		private T Data { get; set; }
		private SymbolRegistry<T> SymbolRegistry { get; set; }

		public TreeInterpreterVisitor(SymbolRegistry<T> symbols)
		{
			SymbolRegistry = symbols;
		}

		public TreeInterpreterVisitor(SymbolRegistry<T> symbols, T data)
		{
			Data = data;
			SymbolRegistry = symbols;
		}
		
		public override object Visit(BinaryExpression ex)
		{
			var left = (double)ex.Left.Visit(this);
			var right = (double)ex.Right.Visit(this);
			var result = double.NaN;

			switch (ex.Operator)
			{
				case Operator.Add:
					return Operator_Add(ex.Left, ex.Right, left, right);
				case Operator.Divide:
					return Operator_Divide(ex.Left, ex.Right, left, right);
				case Operator.Multiply:
					return Operator_Multiply(ex.Left, ex.Right, left, right);
				case Operator.Subtract:
					return Operator_Subtract(ex.Left, ex.Right, left, right);
			}

			return result;
		}

		public override object Visit(UnaryExpression ex)
		{
			return -(double)ex.Expression.Visit(this);
		}

		public override object Visit(IdentifierExpression ex)
		{
			if (SymbolRegistry == null)
			{
				throw new ExpressionException("Cannot resolve identifier - symbol table is null");
			}

			return SymbolRegistry.Evaluate(Data, ex.Identifier);
		}

		public override object Visit(NumberLiteralExpression ex)
		{
			return ex.Value;
		}

		public override object Visit(PercentLiteralExpression ex)
		{
			return ex.Value;
		}

		public override object Visit(CurrencyLiteralExpression ex)
		{
			return ex.Value;
		}


		private double Operator_Add(Expression left, Expression right, double leftValue, double rightValue)
		{
			// Special cases:
			//		- Adding a % to a number or currency

			if ((left.DataType == DataType.Percent || right.DataType == DataType.Percent) && left.DataType != right.DataType)
			{
				double percentValue;
				double nonPercent;

				if (left.DataType == DataType.Percent)
				{
					percentValue = leftValue; 
					nonPercent = rightValue;
				}
				else
				{
					percentValue = rightValue;
					nonPercent = leftValue;
				}

				// So if we say 100 + 10%, what we're really saying is (100 + (100 * 10%)) = 110
				return nonPercent + (nonPercent * percentValue);
			}

			return leftValue + rightValue;
		}

		private double Operator_Subtract(Expression left, Expression right, double leftValue, double rightValue)
		{
			// Special cases:
			//		- Subtracting a % to a number or currency

			if ((left.DataType == DataType.Percent || right.DataType == DataType.Percent) && left.DataType != right.DataType)
			{
				double percentValue;
				double nonPercent;

				if (left.DataType == DataType.Percent)
				{
					percentValue = leftValue;
					nonPercent = rightValue;
				}
				else
				{
					percentValue = rightValue;
					nonPercent = leftValue;
				}

				// So if we say 100 - 10%, what we're really saying is (100 - (100 * 10%)) = 110
				return nonPercent - (nonPercent * percentValue);
			}

			return leftValue - rightValue;
		}

		private double Operator_Multiply(Expression left, Expression right, double leftValue, double rightValue)
		{
			return leftValue * rightValue;
		}

		private double Operator_Divide(Expression left, Expression right, double leftValue, double rightValue)
		{
			if (rightValue == 0)
			{
				return double.NaN;
			}

			return leftValue / rightValue;
		}
	}
}
