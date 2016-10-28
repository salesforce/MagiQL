namespace MagiQL.Expressions.Model
{
	public class PercentLiteralExpression : Expression
	{
		public double Value { get; set; }

		public PercentLiteralExpression()
		{

		}

		public PercentLiteralExpression(double val)
		{
			Value = val;
		}
		
		public override DataType DataType
		{
			get
			{
				return DataType.Percent;
			}
		}

		public override object Visit(Visitor visitor)
		{
			return visitor.Visit(this);
		}

		public override string ToString()
		{
			return (Value * 100).ToString("0.00") + "%";
		}
	}
}
