using System;

namespace MagiQL.Expressions
{
	public class ExpressionException : Exception
	{
		public int Position { get; private set; }
		public string Text { get; private set; }
		public ExpressionException(string message) : base(message) {}
		public ExpressionException(string message, int position, string text) : base(message) 
		{
			Position = position;
			Text = text;
		}

		public ExpressionException(string message, int position)
			: base(message)
		{
			Position = position;
		}
	}
}
