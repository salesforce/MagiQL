namespace MagiQL.Expressions.Model
{
	public abstract class Visitor
	{
		public virtual object Visit(Expression ex)
		{
            var result = ex.Visit(this);
		    return result;
		}

		public virtual object Visit(BinaryExpression ex)
		{
			return null;
		}

		public virtual object Visit(UnaryExpression ex)
		{
			return null;
		}

		public virtual object Visit(NumberLiteralExpression ex)
		{
			return null;
		}

		public virtual object Visit(PercentLiteralExpression ex)
		{
			return null;
		}

		public virtual object Visit(CurrencyLiteralExpression ex)
		{
			return null;
		}

		public virtual object Visit(BooleanLiteralExpression ex)
		{
			return null;
		}

		public virtual object Visit(IdentifierExpression ex)
		{
			return null;
		}
	}
}
