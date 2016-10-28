namespace MagiQL.Expressions.Model
{
	public class Token
	{
		public string Value { get; set; }
		public double DoubleValue { get; set; }
		public TokenType Type { get; set; }

		public override string ToString()
		{
			return Type + ": " + Value;
		}
	}
}
