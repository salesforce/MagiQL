using MagiQL.Expressions.Model;
using NUnit.Framework;

namespace MagiQL.Expressions.Tests.Unit
{
	[TestFixture(Category="Language")]
	public class ParserTests
	{
		//private string[] strings = new string[]
		//{
		//    "1.4%",
		//    "$.50",
		//    "(Spend / Impressions) * CTR",
				
		//    "$2 * 2",
		//    "Spend / Clicks",
				
		//    "Spend * 2",
		//    "Spend / 1000",
		//    "1.4%",
		//    "$2.50",
		//    "Clicks * $1.50",
		//    "Spend + (Spend * 50%)",
		//    "Spend / 2",
		//    "Spend + 10",
		//    "(Clicks + Impressions)",
		//    "Bid + 10%",
		//    "((bid / 2) + 10% + $1.50)",
		//
		//		true != false
		//		true == false
		//		true > false
		//		true < false
		//		true >= false
		//		true <= false
		//		!true
		//		true && false
		//		true || false
		//};

		#region Simple Literals
		[Test]
		public void TestLiteralPercent()
		{
			var expression = new Parser("1.4%").Parse();

			Assert.IsInstanceOf<PercentLiteralExpression>(expression);
			Assert.AreEqual(0.014D, (expression as PercentLiteralExpression).Value, 0.01);
		}

		[Test]
		public void TestLiteralCurrencyNormal()
		{
			var expression = new Parser("$0.50").Parse();

			Assert.IsInstanceOf<CurrencyLiteralExpression>(expression);
			Assert.AreEqual(50D, (expression as CurrencyLiteralExpression).Value);
		}

		[Test]
		public void TestLiteralCurrencyNoLeadingZero()
		{
			var expression = new Parser("$.50").Parse();

			Assert.IsInstanceOf<CurrencyLiteralExpression>(expression);
			Assert.AreEqual(50D, (expression as CurrencyLiteralExpression).Value);
		}

		[Test]
		public void TestLiteralCurrencyNoDecimals()
		{
			var expression = new Parser("$1").Parse();

			Assert.IsInstanceOf<CurrencyLiteralExpression>(expression);
			Assert.AreEqual(100D, (expression as CurrencyLiteralExpression).Value);
		}

		[Test]
		public void TestLiteralNumberNormal()
		{
			var expression = new Parser("1").Parse();

			Assert.IsInstanceOf<NumberLiteralExpression>(expression);
			Assert.AreEqual(1D, (expression as NumberLiteralExpression).Value);
		}

		[Test]
		public void TestLiteralNumberBigger()
		{
			var expression = new Parser("1000000").Parse();

			Assert.IsInstanceOf<NumberLiteralExpression>(expression);
			Assert.AreEqual(1000000D, (expression as NumberLiteralExpression).Value);
		}

		[Test]
		public void TestLiteralNumberDecimals()
		{
			var expression = new Parser("1.5").Parse();

			Assert.IsInstanceOf<NumberLiteralExpression>(expression);
			Assert.AreEqual(1.5D, (expression as NumberLiteralExpression).Value);
		}

		[Test]
		public void TestLiteralNumberMoreDecimals()
		{
			var expression = new Parser("1.50005").Parse();

			Assert.IsInstanceOf<NumberLiteralExpression>(expression);
			Assert.AreEqual(1.50005D, (expression as NumberLiteralExpression).Value);
		}

		[Test]
		public void TestLiteralTrue()
		{
			var expression = new Parser("true").Parse();

			Assert.IsInstanceOf<BooleanLiteralExpression>(expression);
			Assert.AreEqual(true, (expression as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestLiteralFalse()
		{
			var expression = new Parser("false").Parse();

			Assert.IsInstanceOf<BooleanLiteralExpression>(expression);
			Assert.AreEqual(false, (expression as BooleanLiteralExpression).Value);
		}
		#endregion

		#region Unary Expressions
		[Test]
		public void Test_Unary_Number()
		{
			var expression = new Parser("-1").Parse();

			Assert.IsInstanceOf<UnaryExpression>(expression);

			var b = expression as UnaryExpression;

			Assert.AreEqual(Operator.Minus, b.Operator);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Expression);

			Assert.AreEqual(1, (b.Expression as NumberLiteralExpression).Value);
		}

		public void Test_Unary_Parens()
		{
			var expression = new Parser("-(1)").Parse();

			Assert.IsInstanceOf<UnaryExpression>(expression);

			var b = expression as UnaryExpression;

			Assert.AreEqual(Operator.Minus, b.Operator);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Expression);

			Assert.AreEqual(1, (b.Expression as NumberLiteralExpression).Value);
		}

		public void Test_Unary_Expression()
		{
			var expression = new Parser("-(3 * 4)").Parse();

			Assert.IsInstanceOf<UnaryExpression>(expression);

			var b = expression as UnaryExpression;

			Assert.AreEqual(Operator.Minus, b.Operator);
			Assert.IsInstanceOf<BinaryExpression>(b.Expression);
		}

		public void Test_Unary_Mixed()
		{
			var expression = new Parser("1*-1").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.Minus, b.Operator);
			Assert.IsInstanceOf<UnaryExpression>(b.Right);
		}
		#endregion

		#region Binary Expression
		[Test]
		public void TestBinaryExpressionSimpleDivide()
		{
			var expression = new Parser("1 / 1").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.Divide, b.Operator);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Left);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

			Assert.AreEqual(1, (b.Left as NumberLiteralExpression).Value);
			Assert.AreEqual(1, (b.Right as NumberLiteralExpression).Value);
		}


		[Test]
		public void TestBinaryExpressionSimpleEquals()
		{
			var expression = new Parser("true == false").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.Equals, b.Operator);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Right);

			Assert.AreEqual(true, (b.Left as BooleanLiteralExpression).Value);
			Assert.AreEqual(false, (b.Right as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleNotEquals()
		{
			var expression = new Parser("true != false").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.NotEquals, b.Operator);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Right);

			Assert.AreEqual(true, (b.Left as BooleanLiteralExpression).Value);
			Assert.AreEqual(false, (b.Right as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleGreater()
		{
			var expression = new Parser("true > false").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.GreaterThan, b.Operator);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Right);

			Assert.AreEqual(true, (b.Left as BooleanLiteralExpression).Value);
			Assert.AreEqual(false, (b.Right as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleLess()
		{
			var expression = new Parser("true < false").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.LessThan, b.Operator);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Right);

			Assert.AreEqual(true, (b.Left as BooleanLiteralExpression).Value);
			Assert.AreEqual(false, (b.Right as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleGreaterEqual()
		{
			var expression = new Parser("true >= false").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.GreaterThanEqualTo, b.Operator);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Right);

			Assert.AreEqual(true, (b.Left as BooleanLiteralExpression).Value);
			Assert.AreEqual(false, (b.Right as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleLessEqual()
		{
			var expression = new Parser("true <= false").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.LessThanEqualTo, b.Operator);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Right);

			Assert.AreEqual(true, (b.Left as BooleanLiteralExpression).Value);
			Assert.AreEqual(false, (b.Right as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleLogicalOr()
		{
			var expression = new Parser("true || false").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.LogicalOr, b.Operator);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Right);

			Assert.AreEqual(true, (b.Left as BooleanLiteralExpression).Value);
			Assert.AreEqual(false, (b.Right as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleLogicalAnd()
		{
			var expression = new Parser("true && false").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.LogicalAnd, b.Operator);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BooleanLiteralExpression>(b.Right);

			Assert.AreEqual(true, (b.Left as BooleanLiteralExpression).Value);
			Assert.AreEqual(false, (b.Right as BooleanLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleDivide2()
		{
			var expression = new Parser("1/1").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.Divide, b.Operator);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Left);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

			Assert.AreEqual(1, (b.Left as NumberLiteralExpression).Value);
			Assert.AreEqual(1, (b.Right as NumberLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionSimpleParens()
		{
			var strings = new[]
			{
				"(1)/(1)",
				"1/1",
				"1 / 1",
				"(1/1)",
				"((1/1))",
				"(((1)/(1)))"
			};

			foreach (var str in strings)
			{
				var expression = new Parser(str).Parse();

				Assert.IsInstanceOf<BinaryExpression>(expression);

				var b = expression as BinaryExpression;

				Assert.AreEqual(Operator.Divide, b.Operator);
				Assert.IsInstanceOf<NumberLiteralExpression>(b.Left);
				Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

				Assert.AreEqual(1, (b.Left as NumberLiteralExpression).Value);
				Assert.AreEqual(1, (b.Right as NumberLiteralExpression).Value);
			}
		}

		[Test] 
		public void TestBinaryExpressionExtraCloseParens()
		{
			var strings = new[]
			{
				"(((1)/(1))))"
			};

		    Assert.Throws<ExpressionException>(() =>
		    {
		        foreach (var str in strings)
		        {
		            var expression = new Parser(str).Parse();
		        }
		    });
		}

		[Test]
		public void TestBinaryExpressionSimpleMultiply()
		{
			var expression = new Parser("$2 * 2").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.Multiply, b.Operator);
			Assert.IsInstanceOf<CurrencyLiteralExpression>(b.Left);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

			Assert.AreEqual(200, (b.Left as CurrencyLiteralExpression).Value);
			Assert.AreEqual(2, (b.Right as NumberLiteralExpression).Value);
		}

		[Test]
		public void TestBinaryExpressionCompoundLeft()
		{
			var strings  = new[] {
				"(Spend / Impressions) * CTR",
				"((Spend / Impressions) * CTR)",
				"(((Spend / Impressions)) * (CTR))",
				"((((Spend) / (Impressions))) * (CTR))"
			};

			foreach (var str in strings)
			{
				var expression = new Parser(str).Parse();

				Assert.IsInstanceOf<BinaryExpression>(expression);

				var b = expression as BinaryExpression;

				Assert.AreEqual(Operator.Multiply, b.Operator);
				Assert.IsInstanceOf<BinaryExpression>(b.Left);
				Assert.IsInstanceOf<IdentifierExpression>(b.Right);

				Assert.AreEqual("CTR", (b.Right as IdentifierExpression).Identifier);

				b = b.Left as BinaryExpression;

				Assert.AreEqual(Operator.Divide, b.Operator);
				Assert.IsInstanceOf<IdentifierExpression>(b.Left);
				Assert.IsInstanceOf<IdentifierExpression>(b.Right);

				Assert.AreEqual("Spend", (b.Left as IdentifierExpression).Identifier);
				Assert.AreEqual("Impressions", (b.Right as IdentifierExpression).Identifier);
			}
		}

		[Test]
		public void TestBinaryExpressionCompoundRight()
		{
			var strings = new[] {
				"Spend + (Spend * 50%)",
				"(Spend + (Spend * 50%))",
				"(Spend + (Spend * (50%)))",
				"(Spend + ((Spend) * 50%))",
				"((Spend) + (Spend * 50%))",
				"((Spend) + ((Spend) * (50%)))",
				"(((Spend) + ((Spend) * (50%))))",

				"Spend+(Spend*50%)",
				"(Spend+(Spend*50%))",
				"(Spend+(Spend*(50%)))",
				"(Spend+((Spend)*50%))",
				"((Spend)+(Spend*50%))",
				"((Spend)+((Spend)*(50%)))",
				"(((Spend)+((Spend)*(50%))))"
			};

			foreach (var str in strings)
			{
				var expression = new Parser(str).Parse();

				Assert.IsInstanceOf<BinaryExpression>(expression);

				var b = expression as BinaryExpression;

				Assert.AreEqual(Operator.Add, b.Operator);
				Assert.IsInstanceOf<IdentifierExpression>(b.Left);
				Assert.IsInstanceOf<BinaryExpression>(b.Right);

				Assert.AreEqual("Spend", (b.Left as IdentifierExpression).Identifier);

				b = b.Right as BinaryExpression;

				Assert.AreEqual(Operator.Multiply, b.Operator);
				Assert.IsInstanceOf<IdentifierExpression>(b.Left);
				Assert.IsInstanceOf<PercentLiteralExpression>(b.Right);

				Assert.AreEqual("Spend", (b.Left as IdentifierExpression).Identifier);
				Assert.AreEqual(0.5, (b.Right as PercentLiteralExpression).Value, 0.01);
			}
		}

		[Test]
		public void TestBinaryExpressionCompoundNoParens()
		{
			var strings = new[] {
				"(1) + (2) + (3)",
				"1 + 2 + 3",
				"(1 + 2) + 3",
				"((1 + 2) + 3)"
			};

			foreach (var str in strings)
			{
				var expression = new Parser(str).Parse();

				Assert.IsInstanceOf<BinaryExpression>(expression);

				var b = expression as BinaryExpression;

				Assert.AreEqual(Operator.Add, b.Operator);
				Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);
				Assert.IsInstanceOf<BinaryExpression>(b.Left);
				Assert.AreEqual(3, (b.Right as NumberLiteralExpression).Value, 0.01);

				b = b.Left as BinaryExpression;

				Assert.AreEqual(Operator.Add, b.Operator);
				Assert.IsInstanceOf<NumberLiteralExpression>(b.Left);
				Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

				//Assert.AreEqual("Spend", (b.Left as IdentifierExpression).Identifier);
				Assert.AreEqual(1, (b.Left as NumberLiteralExpression).Value, 0.01);
				Assert.AreEqual(2, (b.Right as NumberLiteralExpression).Value, 0.01);
			}
		}
		#endregion

		#region Operator Precedence
		[Test]
		public void Test_Precedence_Multiply_1()
		{
			var expression = new Parser("1 * 2 + 3").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.Add, b.Operator);
			Assert.IsInstanceOf<BinaryExpression>(b.Left);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

			Assert.AreEqual(3, (b.Right as NumberLiteralExpression).Value);
		}

		[Test]
		public void Test_Precendence_Multiply_2()
		{
			var expression = new Parser("(1 * 2) + 3").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.Add, b.Operator);
			Assert.IsInstanceOf<BinaryExpression>(b.Left);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

			Assert.AreEqual(3, (b.Right as NumberLiteralExpression).Value);

			b = b.Left as BinaryExpression;

			Assert.AreEqual(Operator.Multiply, b.Operator);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Left);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

			Assert.AreEqual(1, (b.Left as NumberLiteralExpression).Value);
			Assert.AreEqual(2, (b.Right as NumberLiteralExpression).Value);
		}

		[Test]
		public void Test_Precendence_Multiply_3()
		{
			var expression = new Parser("1 + 2 * 3").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var b = expression as BinaryExpression;

			Assert.AreEqual(Operator.Add, b.Operator);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Left);
			Assert.IsInstanceOf<BinaryExpression>(b.Right);

			Assert.AreEqual(1, (b.Left as NumberLiteralExpression).Value);

			b = b.Right as BinaryExpression;

			Assert.AreEqual(Operator.Multiply, b.Operator);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Left);
			Assert.IsInstanceOf<NumberLiteralExpression>(b.Right);

			Assert.AreEqual(2, (b.Left as NumberLiteralExpression).Value);
			Assert.AreEqual(3, (b.Right as NumberLiteralExpression).Value);
		}
		#endregion

		#region Exceptions / Errors
		[Test] 
		public void Test_Error_Binary_Right_Missing()
		{
		    Assert.Throws<ExpressionException>(() =>
		    {
		        new Parser("1+").Parse();
		    });
		}

		[Test] 
		public void Test_Error_Binary_Left_Missing()
		{
		    Assert.Throws<ExpressionException>(() =>
		    {
		        new Parser("+1").Parse();
		    });
		}

		[Test]
		public void Test_Error_Binary_ExtraParens1()
		{
		    Assert.Throws<ExpressionException>(() =>
		    {
		        new Parser("(").Parse();
		    });
		}

		[Test] 
		public void Test_Error_Binary_ExtraParens2()
		{
		    Assert.Throws<ExpressionException>(() =>
		    {
		        new Parser(")").Parse();
		    });
		}

		[Test]
		public void Test_Error_Binary_EmptyParens()
		{
		    Assert.Throws<ExpressionException>(() =>
		    {
		        new Parser("()").Parse();
		    });
		}

		[Test] 
		public void Test_Error_Binary_ExtraParens3()
		{
		    Assert.Throws<ExpressionException>(() =>
		    {
		        new Parser("1+(2+").Parse();
		    });
		}
		#endregion
	}
}
