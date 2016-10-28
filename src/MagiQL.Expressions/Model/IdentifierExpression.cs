namespace MagiQL.Expressions.Model
{
	public class IdentifierExpression : Expression
	{
		public string Identifier { get; set; }

		public IdentifierExpression()
		{

		}

		public IdentifierExpression(string identifier)
		{
			Identifier = identifier;
		}

		public override object Visit(Visitor visitor)
		{
			return visitor.Visit(this);
		}

		public override string ToString()
		{
			return Identifier ?? "[]";
		}
	}
}
