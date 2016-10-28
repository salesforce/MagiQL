using System;
using System.Collections.Generic;
using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class Lexer
	{
		public string Text { get; private set; }
		public int Position { get; private set; }

		public Lexer(string text)
		{
			Text = text ?? "";

			Position = 0;
		}

		public List<Token> Scan()
		{
			var result = new List<Token>();

			var token = Next();

			while (token != null)
			{
				if (token.Type != TokenType.WhiteSpace)
				{
					result.Add(token);
				}

				token = Next();
			}

			return result;
		}

		public Token Next()
		{
			var c = PeekChar();

			if (c == 0)
			{
				return null;
			}
		    if (Char.IsWhiteSpace(c))
		    {
		        var current = c.ToString();

		        while (Char.IsWhiteSpace(c))
		        {
		            current += NextChar();
		            c = PeekChar();
		        }

		        return new Token
		        {
		            Type = TokenType.WhiteSpace,
		            Value = current
		        };
		    }
		    if (c == '(')
		    {
		        return new Token
		        {
		            Type = TokenType.OpenParens,
		            Value = NextChar().ToString()
		        };
		    }
		    if (c == ')')
		    {
		        return new Token
		        {
		            Type = TokenType.CloseParens,
		            Value = NextChar().ToString()
		        };
		    }
		    if (IsOperator(c))
		    {
		        return new Token
		        {
		            Type = TokenType.Operator,
		            Value = NextChar().ToString()
		        };
		    }
		    if (Char.IsNumber(c))
		    {
		        // Parse the rest of the number
		        var number = ParseNumberToken();

		        return number;
		    }
		    if (IsCurrency(c))
		    {
		        // Skip symbol
		        NextChar();

		        var number = ParseNumber();
		        var currency = double.Parse(number);
		        currency *= 100;

		        return new Token
		        {
		            Type = TokenType.Currency,
		            Value = c + number,
		            DoubleValue = ((int)currency),
		        };
		    }
		    if (Char.IsLetter(c) || c == '_')
		    {
		        var value = ParseIdentifier();

		        if (value == "true" || value == "false")
		        {
		            return new Token
		            {
		                Type = TokenType.Keyword,
		                Value = value
		            };
		        }
		        if (value.ToLower() == "and")
		        {
		            return new Token
		            {
		                Type = TokenType.Operator,
		                Value = "&&"
		            };
		        }
		        if (value.ToLower() == "or")
		        {
		            return new Token
		            {
		                Type = TokenType.Operator,
		                Value = "||"
		            };
		        }
		        return new Token
		        {
		            Type = TokenType.Identifier,
		            Value = value
		        };
		    }
		    if (c == '>')
		    {
		        NextChar();
		        var next = PeekChar();

		        if (next == '=')
		        {
		            NextChar();
		            return new Token
		            {
		                Type = TokenType.Operator,
		                Value = ">="
		            };
		        }
		        return new Token
		        {
		            Type = TokenType.Operator,
		            Value = ">"
		        };
		    }
		    if (c == '<')
		    {
		        NextChar();
		        var next = PeekChar();

		        if (next == '=')
		        {
		            NextChar();
		            return new Token
		            {
		                Type = TokenType.Operator,
		                Value = "<="
		            };
		        }
		        return new Token
		        {
		            Type = TokenType.Operator,
		            Value = "<"
		        };
		    }
		    if (c == '=')
		    {
		        NextChar();
		        var next = PeekChar();

		        if (next == '=')
		        {
		            NextChar();
		            return new Token
		            {
		                Type = TokenType.Operator,
		                Value = "=="
		            };
		        }
		        InvalidTerm(next);
		    }
		    else if (c == '!')
		    {
		        NextChar();
		        var next = PeekChar();

		        if (next == '=')
		        {
		            NextChar();
		            return new Token
		            {
		                Type = TokenType.Operator,
		                Value = "!="
		            };
		        }
		        return new Token
		        {
		            Type = TokenType.Operator,
		            Value = "!"
		        };
		    }
		    else if (c == '&')
		    {
		        NextChar();
		        var next = PeekChar();

		        if (next == '&')
		        {
		            NextChar();
		            return new Token
		            {
		                Type = TokenType.Operator,
		                Value = "&&"
		            };
		        }
		        InvalidTerm(next);
		    }
		    else if (c == '|')
		    {
		        NextChar();
		        var next = PeekChar();

		        if (next == '|')
		        {
		            NextChar();
		            return new Token
		            {
		                Type = TokenType.Operator,
		                Value = "||"
		            };
		        }
		        InvalidTerm(next);
		    }
		    else
		    {
		        InvalidTerm(c);
		    }

		    return null;
		}

		private Token ParseNumberToken()
		{
			// Ignore minuses
			var c = PeekChar();
			var current = "";
			var result = new Token
			{
				Type = TokenType.Number
			};

			double d;

			while (c > 0)
			{
				if (Char.IsNumber(c) || c == '.')
				{
					current += NextChar();
				}
				else
				{
					break;
				}

				c = PeekChar();
			}

			if (!double.TryParse(current, out d))
			{
				Error("Could not parse '" + current + "' as a number");
			}

			if (c == '%')
			{
				// Aha! Special case.
				result.Type = TokenType.Percent;
				result.Value = current + NextChar();
				result.DoubleValue = d / 100.0;
			}
			else
			{
				result.Value = current;
				result.DoubleValue = d;
			}

			return result;
		}

		private string ParseNumber()
		{
			// Ignore minuses
			var c = PeekChar();
			var current = "";
			double result;

			while (c > 0)
			{
				if (Char.IsNumber(c) || c == '.')
				{
					current += NextChar();
				}
				else
				{
					break;
				}

				c = PeekChar();
			}

			if (!double.TryParse(current, out result))
			{
				Error("Could not parse '" + current + "' as a number");
			}

			if (c == '%')
			{

			}
			
			return current;
		}

		private void Error(string message)
		{
			throw new ExpressionException(message, Position, Text);
		}

		private void InvalidTerm(char c)
		{
			Error("Invalid token '" + c + "'");
		}

		private string ParseIdentifier()
		{
			var c = PeekChar();
			var current = "";

			while (c > 0)
			{
				if (Char.IsNumber(c) || c == '.' || Char.IsLetter(c) || c == '_')
				{
					current += NextChar();
				}
				else
				{
					break;
				}

				c = PeekChar();
			}
			
			return current;
		}

		private char NextChar()
		{
			if ((Position + 1) > Text.Length)
			{
				return (char)0;
			}

			var result = Text[Position];
			Position = Position + 1;

			return result;
		}

		private char PeekChar()
		{
			if ((Position) >= Text.Length)
			{
				return (char)0;
			}

			var result = Text[Position];

			return result;
		}

		private static bool IsOperator(char c)
		{
			switch (c)
			{
				case '+':
				case '-':
				case '*':
				case '/':
					return true;
			}

			return false;
		}

		private static bool IsCurrency(char c)
		{
			switch (c)
			{
				case '£':
				case '$':
				case '€':
					return true;
			}

			return false;
		}
	}
}
