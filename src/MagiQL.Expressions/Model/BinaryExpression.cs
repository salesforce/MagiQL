namespace MagiQL.Expressions.Model
{
	public class BinaryExpression : Expression
	{
		public Expression Left { get; set; }
		public Expression Right { get; set; }
		public Operator Operator { get; set; }
		
		public BinaryExpression()
		{
		}

		public BinaryExpression(Expression left, Expression right, Operator op)
		{
			Left = left;
			Right = right;
			Operator = op;
		}

		public BinaryExpression(Expression left, Expression right)
		{
			Left = left;
			Right = right;
		}

		public override object Visit(Visitor visitor)
		{
            var result = visitor.Visit(this);
		    return result;
		}

		public override string ToString()
		{
			var left = Left != null ? Left.ToString() : "[]";
			var right = Right != null ? Right.ToString() : "[]";

			return "(" + left + " " + GetOperator(Operator) + " " + right + ")";
		}

		private string GetOperator(Operator op)
		{
			switch (op)
			{
				case Operator.Add: return "+";
				case Operator.Subtract: return "-";
				case Operator.Multiply: return "*";
				case Operator.Divide: return "/";
				case Operator.Equals: return "==";
				case Operator.GreaterThan: return ">";
				case Operator.GreaterThanEqualTo: return ">=";
				case Operator.LessThan: return "<";
				case Operator.LessThanEqualTo: return "<=";
				case Operator.LogicalAnd: return "&&";
				case Operator.LogicalOr: return "||";
				case Operator.NotEquals: return "!=";
			}

			return "?";
		}
	}
}
