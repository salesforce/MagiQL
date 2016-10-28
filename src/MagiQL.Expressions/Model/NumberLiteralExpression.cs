namespace MagiQL.Expressions.Model
{
	public class NumberLiteralExpression : Expression
	{
		public double Value { get; set; }

		public NumberLiteralExpression()
		{

		}

		public NumberLiteralExpression(double value)
		{
			Value = value;
		}
		
		public override DataType DataType
		{
			get
			{
				return DataType.Number;
			}
		}

		public override object Visit(Visitor visitor)
		{
			return visitor.Visit(this);
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}
