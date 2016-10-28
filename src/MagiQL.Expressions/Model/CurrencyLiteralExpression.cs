namespace MagiQL.Expressions.Model
{
	public class CurrencyLiteralExpression : Expression
	{
		public double Value { get; set; }

		public CurrencyLiteralExpression()
		{

		}

		public CurrencyLiteralExpression(double val)
		{
			Value = val;
		}


		public override DataType DataType
		{
			get
			{
				return DataType.Currency;
			}
		}

		public override object Visit(Visitor visitor)
		{
			return visitor.Visit(this);
		}

		public override string ToString()
		{
			return "$" + (Value / 100).ToString("0.00");
		}
	}
}
