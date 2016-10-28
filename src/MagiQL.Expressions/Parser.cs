using System.Collections.Generic;
using System.Linq;
using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class Parser
	{
		private Token[] Tokens { get; set; }
		private int Position { get; set; }
		private string Text { get; set; }
		private string TextParsed { get; set; }
		private Token CurrentToken { get; set; }

		public Parser(string text, IEnumerable<Token> tokens)
		{
			Tokens = tokens.ToArray();
			Position = 0;
			Text = text;
		}

		public Parser(string text)
		{
			Tokens = new Lexer(text).Scan().ToArray();
			Position = 0;
			Text = text;
		}

		public Expression Parse()
		{
			var result = ParseExpression(ParsePrimary());

			var next = Next();

			if (next != null)
			{
				Error("Expected end of expression but found '" + next.Value + "'");
			}

			return result;
		}

		private Expression ParseExpression(Expression lhs)
		{
			var result = ParseExpression(lhs, 0);

			var token = Peek();

			if (token != null && token.Type == TokenType.Operator)
			{
				Next();
				result = new BinaryExpression(result, ParseExpression(ParsePrimary()), GetOperator(token.Value));
			}

			return result;
		}

		private Expression ParseExpression(Expression lhs, int precedence)
		{
			var token = Peek();

			while (token != null && token.Type == TokenType.Operator && GetPrecedence(token.Value) >= precedence)
			{
				var op = Next();
				var rhs = ParsePrimary();

				precedence = GetPrecedence(op.Value);
				token = Peek();

				while (token != null && token.Type == TokenType.Operator && GetPrecedence(token.Value) > precedence)
				{
					rhs = ParseExpression(rhs, GetPrecedence(token.Value));
					token = Peek();
				}

				if (rhs == null)
				{
					Error("Expected expression");
				}

				lhs = new BinaryExpression(lhs, rhs, GetOperator(op.Value));
			}

			return lhs;
		}

		private Expression ParsePrimary()
		{
			Expression result = null;

			var token = Peek();

			if (token != null)
			{
				switch (token.Type)
				{
					case TokenType.OpenParens:
						Expect(TokenType.OpenParens);
					
						result = ParseExpression(ParsePrimary());

						Expect(TokenType.CloseParens);
						break;
					case TokenType.Currency:
						return new CurrencyLiteralExpression(Next().DoubleValue);
					case TokenType.Number:
						return new NumberLiteralExpression(Next().DoubleValue);
					case TokenType.Identifier:
						return new IdentifierExpression(Next().Value);
					case TokenType.Percent:
						return new PercentLiteralExpression(Next().DoubleValue);
					case TokenType.Keyword:
						// Currently only 'true' and 'false' keywords are supported
						return new BooleanLiteralExpression(Next().Value == "true");
					case TokenType.Operator:
						// Only unary operators should appear here - the rest are in binary expressions
						if (token.Value == "-")
						{
							Next();

							return new UnaryExpression(ParseExpression(ParsePrimary()), Operator.Minus);
						}
				        if (token.Value == "!")
				        {
				            Next();

				            return new UnaryExpression(ParseExpression(ParsePrimary()), Operator.Negate);
				        }
				        Found(token);
				        break;
					default:
						Found(token);
						break;
				}
			}

			return result;
		}
		
		private Token Next()
		{
			if (Position >= Tokens.Length)
			{
				CurrentToken = null;
				return null;
			}

			var result = Tokens[Position];

			Position = Position + 1;
			CurrentToken = result;

			if (CurrentToken != null)
			{
				if (TextParsed == null) TextParsed = "";
				TextParsed += CurrentToken.Value;
			}

			return result;
		}

		private Token Peek()
		{
			if (Position >= Tokens.Length)
			{
				return null;
			}

			var result = Tokens[Position];

			return result;
		}

		private void Error(string message)
		{
			throw new ExpressionException(message, Position);
		}

		private void Found(Token found)
		{
			Error("Invalid expression term '" + found.Value + "'");

		}

		private void Expect(TokenType type)
		{
			var token = Next();

			if (token == null)
			{
				var str = type.ToString();
				if (type == TokenType.CloseParens)
				{
					str = ")";
				}

				Error("Expected '" + str + "' but end of expression found");
			}
			else if (token.Type != type)
			{
				Expected(type, token);
			}
		}

		private void Expected(TokenType expected, Token found)
		{
			var str = expected.ToString();
			if (expected == TokenType.CloseParens)
			{
				str = ")";
			}

			Error("Invalid token '" + found.Value + "', expected " + str);
		}

		private int GetPrecedence(string c)
		{
			switch (c)
			{
				case "||": return 1;
				case "&&": return 1;

				case "==": return 2;
				case "!=": return 2;

				case ">": return 3;
				case "<": return 3;
				case ">=": return 3;
				case "<=": return 3;
				
				case "+": return 4;
				case "-": return 4;

				case "*": return 5;
				case "/": return 5;
			}

			return 0;
		}

		private Operator GetOperator(string op)
		{
			switch (op)
			{
				case "-":
					return Operator.Subtract;
				case "+":
					return Operator.Add;
				case "*":
					return Operator.Multiply;
				case "/":
					return Operator.Divide;
				case "!":
					return Operator.Negate;
				case "==":
					return Operator.Equals;
				case "!=":
					return Operator.NotEquals;
				case "&&":
					return Operator.LogicalAnd;
				case "||":
					return Operator.LogicalOr;
				case ">":
					return Operator.GreaterThan;
				case "<":
					return Operator.LessThan;
				case ">=":
					return Operator.GreaterThanEqualTo;
				case "<=":
					return Operator.LessThanEqualTo;
			}

			return Operator.None;
		}
	}
}
