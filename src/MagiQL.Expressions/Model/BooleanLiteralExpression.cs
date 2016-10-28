namespace MagiQL.Expressions.Model
{
	public class BooleanLiteralExpression : Expression
	{
		public bool Value { get; set; }

		public BooleanLiteralExpression()
		{

		}

		public BooleanLiteralExpression(bool value)
		{
			Value = value;
		}

		public override DataType DataType
		{
			get
			{
				return DataType.Boolean;
			}
		}

		public override object Visit(Visitor visitor)
		{
			return visitor.Visit(this);
		}

		public override string ToString()
		{
			return Value.ToString().ToLower();
		}
	}
}
