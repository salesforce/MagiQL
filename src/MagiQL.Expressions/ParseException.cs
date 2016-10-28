using System;

namespace MagiQL.Expressions
{
	public class ParseException : Exception
	{
		public int Position { get; private set; }
		public string Text { get; private set; }
		public ParseException(string message) : base(message) {}
		public ParseException(string message, int position, string text) : base(message) 
		{
			Position = position;
			Text = text;
		}

		public ParseException(string message, int position)
			: base(message)
		{
			Position = position;
		}
	}
}
