namespace MagiQL.Expressions.Model
{
	public class UnaryExpression : Expression
	{
		public Expression Expression { get; set; }
		public Operator Operator { get; set; }

		public UnaryExpression()
		{
			Operator = Operator.None;
		}
		
		public UnaryExpression(Expression expression, Operator op)
		{
			Expression = expression;
			Operator = op;
		}

		public override object Visit(Visitor visitor)
		{
			return visitor.Visit(this);
		}

		public override string ToString()
		{
			var expr = Expression != null ? Expression.ToString() : "[]";

			return GetOperator(Operator) + expr;
		}

		private string GetOperator(Operator op)
		{
			switch (op)
			{
				case Operator.Minus: return "-";
				case Operator.Negate: return "!";
			}

			return "?";
		}
	}
}
