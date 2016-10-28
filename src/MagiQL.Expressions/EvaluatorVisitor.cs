using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class EvaluatorVisitor<T> : Visitor
	{
		private T Data { get; set; }
		private SymbolRegistry<T> SymbolRegistry { get; set; }

		public EvaluatorVisitor(SymbolRegistry<T> symbols, T data)
		{
			Data = data;
			SymbolRegistry = symbols;
		}

		public override object Visit(BinaryExpression ex)
		{
			var left = ex.Left.Visit(this);
			var right = ex.Right.Visit(this);
			object result = null;

			switch (ex.Operator)
			{
				case Operator.Add:
                    result = Operator_Add(ex.Left, ex.Right, left, right);
			        break;
				case Operator.Divide:
                    result = Operator_Divide(ex.Left, ex.Right, left, right);
			        break;
				case Operator.Multiply:
                    result = Operator_Multiply(ex.Left, ex.Right, left, right);
			        break;
				case Operator.Subtract:
                    result = Operator_Subtract(ex.Left, ex.Right, left, right);
			        break;

				case Operator.Equals:
				case Operator.NotEquals:
				case Operator.GreaterThan:
				case Operator.GreaterThanEqualTo:
				case Operator.LessThan:
				case Operator.LessThanEqualTo:
					result = Operator_Comparison(ex.Operator, ex.Left, ex.Right, left, right);
			        break;

				case Operator.LogicalAnd:
				case Operator.LogicalOr:
					var leftBool = CastBoolean(ex.Left, left);
					var rightBool = CastBoolean(ex.Right, right);
					result = Operator_Logical(ex.Operator, leftBool, rightBool);
					break;
			}

			return result;
		}

		public override object Visit(UnaryExpression ex)
		{
			object val = ex.Expression.Visit(this);

			switch (ex.Operator)
			{
				case Operator.Minus:
					return -CastNumber(ex, val);
				case Operator.Negate:
					return !CastBoolean(ex, val);
			}

			throw new ExpressionException("Unknown unary operator '" + ex.Operator.ToString() + "'");
		}

		public override object Visit(IdentifierExpression ex)
		{
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

		public override object Visit(BooleanLiteralExpression ex)
		{
			return ex.Value;
		}

		private double Operator_Add(Expression left, Expression right, object leftValue, object rightValue)
		{
			double leftDouble = CastNumber(left, leftValue);
			double rightDouble = CastNumber(right, rightValue);

			// Special cases:
			//		- Adding a % to a number or currency

			if ((left.DataType == DataType.Percent || right.DataType == DataType.Percent) && left.DataType != right.DataType)
			{
				double percentValue;
				double nonPercent;

				if (left.DataType == DataType.Percent)
				{
					percentValue = leftDouble;
					nonPercent = rightDouble;
				}
				else
				{
					percentValue = rightDouble;
					nonPercent = leftDouble;
				}

				// So if we say 100 + 10%, what we're really saying is (100 + (100 * 10%)) = 110
				return nonPercent + (nonPercent * percentValue);
			}

			return leftDouble + rightDouble;
		}

		private double Operator_Subtract(Expression left, Expression right, object leftValue, object rightValue)
		{
			double leftDouble = CastNumber(left, leftValue);
			double rightDouble = CastNumber(right, rightValue);

			// Special cases:
			//		- Subtracting a % to a number or currency

			if ((left.DataType == DataType.Percent || right.DataType == DataType.Percent) && left.DataType != right.DataType)
			{
				double percentValue;
				double nonPercent;

				if (left.DataType == DataType.Percent)
				{
					percentValue = leftDouble;
					nonPercent = rightDouble;
				}
				else
				{
					percentValue = rightDouble;
					nonPercent = leftDouble;
				}

				// So if we say 100 - 10%, what we're really saying is (100 - (100 * 10%)) = 110
				return nonPercent - (nonPercent * percentValue);
			}

			return leftDouble - rightDouble;
		}

		private double Operator_Multiply(Expression left, Expression right, object leftValue, object rightValue)
		{
			double leftDouble = CastNumber(left, leftValue);
			double rightDouble = CastNumber(right, rightValue);

			return leftDouble * rightDouble;
		}

		private double Operator_Divide(Expression left, Expression right, object leftValue, object rightValue)
		{
			double leftDouble = CastNumber(left, leftValue);
			double rightDouble = CastNumber(right, rightValue);

			if (rightDouble == 0)
			{
				return double.NaN;
			}

			return leftDouble / rightDouble;
		}

		private bool Operator_Logical(Operator op, bool left, bool right)
		{
			switch (op)
			{
				case Operator.LogicalAnd:
					return left && right;
				case Operator.LogicalOr:
					return left || right;
				default:
					throw new ExpressionException("Unknown logical operator '" + op.ToString() + "'");
			}
		}

		private bool Operator_Comparison(Operator op, Expression left, Expression right, object leftValue, object rightValue)
		{
			double leftDouble = CastNumber(left, leftValue);
			double rightDouble = CastNumber(right, rightValue);

			switch (op)
			{
				case Operator.Equals:
					return leftDouble == rightDouble;
				case Operator.LessThan:
					return leftDouble < rightDouble;
				case Operator.LessThanEqualTo:
					return leftDouble <= rightDouble;
				case Operator.GreaterThan:
					return leftDouble > rightDouble;
				case Operator.GreaterThanEqualTo:
					return leftDouble >= rightDouble;
				case Operator.NotEquals:
					return leftDouble != rightDouble;
				default:
					throw new ExpressionException("Unknown comparison operator '" + op.ToString() + "'");
			}
		}

		private double CastNumber(Expression expression, object value)
		{
			if (value is double)
			{
				return (double)value;
			}
		    if (value is bool)
		    {
		        return ((bool)value) ? 1 : 0;
		    }

		    return double.NaN;
		}

		private bool CastBoolean(Expression expression, object value)
		{
			if (value is double)
			{
				return (double)value != 0;
			}
		    if (value is bool)
		    {
		        return (bool)value;
		    }

		    return false;
		}
	}
}
