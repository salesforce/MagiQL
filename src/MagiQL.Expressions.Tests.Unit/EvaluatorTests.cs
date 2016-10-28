using System;
using MagiQL.Expressions.Model;
using NUnit.Framework;

namespace MagiQL.Expressions.Tests.Unit
{
	[TestFixture(Category = "Language")]
	public class EvaluatorTests
	{
		#region Simple Literals
		[Test]
		public void TestSimpleLiterals_Add()
		{
			TestExpression("1+1", 2);			// Number	+	Number	=	Number
			TestExpression("1+$0.5", 51);		// Number	+	Currency=	Currency
			TestExpression("1+100%", 2);		// Number	+	Percent	=	Number
			TestExpression("$1+$1", 200);		// Currency	+	Currency=	Currency
			TestExpression("$0.5+1", 51);		// Currency	+	Number	=	Currency
			TestExpression("$0.5+10%", 55);		// Currency	+	Percent	=	Currency
			TestExpression("1%+1%", 0.02);		// Percent	+	Percent	=	Percent
			TestExpression("10%+100", 110);		// Percent	+	Number	=	Number
			TestExpression("10%+$100", 11000);	// Percent	+	Currency=	Currency
		}

		[Test]
		public void TestSimpleLiterals_Subtract()
		{
			TestExpression("1-1", 0);			// Number	-	Number	=	Number
			TestExpression("100-$0.5", 50);		// Number	-	Currency=	Currency
			TestExpression("100-10%", 90);		// Number	-	Percent	=	Number
			TestExpression("$1-$0.5", 50);		// Currency	-	Currency=	Currency
			TestExpression("$0.5-1", 49);		// Currency	-	Number	=	Currency
			TestExpression("$100-10%", 9000);	// Currency	-	Percent	=	Currency
			TestExpression("1%-1%", 0);			// Percent	-	Percent	=	Percent
			TestExpression("10%-100", 90);		// Percent	-	Number	=	Number
			TestExpression("10%-$100", 9000);	// Percent	-	Currency=	Currency
		}

		[Test]
		public void TestSimpleLiterals_Multiply()
		{
			TestExpression("1*2", 2);			// Number	*	Number	=	Number
			TestExpression("100*$0.5", 5000);	// Number	*	Currency=	Currency
			TestExpression("100*10%", 10);		// Number	*	Percent	=	Number
			// Currency	*	Currency=	Unknown
			TestExpression("$0.5*2", 100);		// Currency	*	Number	=	Currency
			TestExpression("$100*10%", 1000);	// Currency	*	Percent	=	Currency
			TestExpression("100%*10%", 0.1);	// Percent	*	Percent	=	Percent
			TestExpression("10%*100", 10);		// Percent	*	Number	=	Number
			TestExpression("10%*$100", 1000);	// Percent	*	Currency=	Currency
		}

		[Test]
		public void TestSimpleLiterals_Divide()
		{
			TestExpression("100/5", 20);		// Number	/	Number	=	Number
			// Number	/	Currency=	Unknown
			TestExpression("100/10%", 1000);	// Number	/	Percent	=	Number
			TestExpression("$100/$10", 10);		// Currency	/	Currency=	Number
			TestExpression("$10/2", 500);		// Currency	/	Number	=	Currency
			TestExpression("$10/10%", 10000);	// Currency	/	Percent	=	Currency
			TestExpression("100%/10%", 10);		// Percent	/	Percent	=	Percent
			TestExpression("10%/2", 0.05);		// Percent	/	Number	=	Percent
			// Percent	/	Currency=	Unknown
		}

		#endregion

		#region Compound Literals
		[Test]
		public void TestCompoundLiterals()
		{
			TestExpression("1+1+1+1+1", 5);
			TestExpression("($90 + $10) * 10%", 1000);
			TestExpression("($10 * 0) + 10%", 0);
			TestExpression("($10 * 0) + $10", 1000);
			TestExpression("($10 * 10) + $10", 11000);
			TestExpression("($10 * 10) + 10%", 11000);
			TestExpression("10% + ($10 * 10)", 11000);

			TestExpression("(1 + (2 + (3 + (4))))", 10);
			TestExpression("(1 * (2 * (3 * (4))))", 24);

			TestExpression("((((1) + 2) + 3) + 4)", 10);
			TestExpression("1 * 2 * 3 * 4", 24);
			TestExpression("((((1) * 2) * 3) * 4)", 24);
		}
		#endregion

		#region Simple Identifiers
		[Test]
		public void TestSimpleIdentifiers()
		{
			TestExpression("Spend", 1);
		}
		#endregion

		#region Unaries
		[Test]
		public void TestUnary_Simple()
		{
			TestExpression("-1", -1);
			TestExpression("1 + -1", 0);
		}

		[Test]
		public void TestUnary_Expression()
		{
			TestExpression("1 - -(9 * 10)", 91);
		}

		[Test]
		public void TestUnary_Expression_Woah()
		{
			TestExpression("1 - -(9 * 10)", 91);
			TestExpression("1--9*10", 91);
		}
		#endregion

		#region Booleans!
		[Test]
		public void Test_Boolean_True()
		{
			TestExpression("true", true);
		}

		[Test]
		public void Test_Boolean_False()
		{
			TestExpression("false", false);
		}

		[Test]
		public void Test_Boolean_Equals()
		{
			TestExpression("1 == 1", true);
			TestExpression("true == true", true);
			TestExpression("false == false", true);
			TestExpression("1% == 1%", true);
			TestExpression("$1 == $1", true);
		}

		[Test]
		public void Test_Boolean_NotEquals()
		{
			TestExpression("1 != 0", true);
			TestExpression("1 != 1", false);
			TestExpression("true != true", false);
			TestExpression("false != false", false);
			TestExpression("1% != 1%", false);
			TestExpression("$1 != $1", false);

			TestExpression("1 != 2", true);
			TestExpression("true != false", true);
			TestExpression("false != true", true);
			TestExpression("1% != 1.1%", true);
			TestExpression("$1 != $1.01", true);
		}

		[Test]
		public void Test_Boolean_Keywords()
		{
			TestExpression("true == true", true);
			TestExpression("true != true", false);

			TestExpression("true == false", false);
			TestExpression("true != false", true);

			TestExpression("false == false", true);
			TestExpression("false != false", false);

			TestExpression("false == true", false);
			TestExpression("false != true", true);
		}

		[Test]
		public void Test_Boolean_Evaluates_True()
		{
			TestExpression("1 == 1", true);
			TestExpression("2 >= 1", true);
			TestExpression("1 <= 2", true);
			TestExpression("1 < 2", true);
			TestExpression("2 > 1", true);
			TestExpression("1 != 2", true);

			TestExpression("1.111 == 1.111", true);
			TestExpression("2.9 >= 1.9", true);
			TestExpression("1.9 <= 2.9", true);
			TestExpression("1.9 < 2.9", true);
			TestExpression("2.9 > 1.9", true);
			TestExpression("1.9 != 2.9", true);

			TestExpression("1 + 1 == 2", true);
			TestExpression("2 * 2 == 2 + 2", true);
		}

		[Test]
		public void Test_Boolean_Evaluates_False()
		{
			TestExpression("1 != 1", false);
			TestExpression("1 >= 2", false);
			TestExpression("2 <= 1", false);
			TestExpression("2 < 1", false);
			TestExpression("1 > 2", false);

			TestExpression("2 < 1", false);
			TestExpression("1 > 2", false);

			TestExpression("1 == 2", false);
		}

		[Test]
		public void Test_Boolean_LessThan()
		{
			TestExpression("1 < 1", false);
			TestExpression("1 < 2", true);

			TestExpression("true < true", false);
			TestExpression("false < false", false);
		}

		[Test]
		public void Test_Boolean_LessThanEqual()
		{
			TestExpression("1 <= 1", true);
			TestExpression("1 <= 2", true);
			TestExpression("2 <= 1", false);

			TestExpression("true <= true", true);
			TestExpression("false <= false", true);
		}

		[Test]
		public void Test_Boolean_GreaterThan()
		{
			TestExpression("1 > 1", false);
			TestExpression("2 > 1", true);

			TestExpression("true > true", false);
			TestExpression("false > false", false);
		}

		[Test]
		public void Test_Boolean_GreaterThanEqual()
		{
			TestExpression("1 >= 1", true);
			TestExpression("2 >= 1", true);

			TestExpression("true >= true", true);
			TestExpression("false >= false", true);
		}

		[Test]
		public void Test_Boolean_LogicalOr()
		{
			TestExpression("true || true", true);
			TestExpression("true || false", true);
			TestExpression("false || true", true);
			TestExpression("false || false", false);
            
            TestExpression("true OR true", true);
            TestExpression("true or false", true);
            TestExpression("false or true", true);
            TestExpression("false or false", false);

			TestExpression("1 || 1", true);
			TestExpression("1 || 0", true);
			TestExpression("0 || 1", true);
			TestExpression("0 || 0", false);
		}

		[Test]
		public void Test_Boolean_LogicalAnd()
		{
			TestExpression("true && true", true);
			TestExpression("true && false", false);
			TestExpression("false && true", false);
			TestExpression("false && false", false);
             
            TestExpression("true AND true", true);
            TestExpression("true and false", false);
            TestExpression("false and true", false);
            TestExpression("false and false", false); 

			TestExpression("1 && 1", true);
			TestExpression("1 && 0", false);
			TestExpression("0 && 1", false);
			TestExpression("0 && 0", false);
		}

		[Test]
		public void Test_Boolean_Equals2()
		{
			TestExpression("true == true", true);
			TestExpression("true == false", false);

			TestExpression("false == false", true);
			TestExpression("false == true", false);

			TestExpression("1 == 1", true);
			TestExpression("1 == true", true);	// JS does this too, it's not just me.
			TestExpression("1 == false", false);
			TestExpression("1 == true", true);
		}

		[Test]
		public void Test_Boolean_Add()
		{
			TestExpression("true + true", 2);
			TestExpression("true + false", 1);
			TestExpression("(1 == 1) + (2 == 2)", 2);
			TestExpression("(1 == 1) + (2 == 2)", 2);
			TestExpression("(1 != 2) + (1 != 2)", 2);
			TestExpression("true + 1", 2);
			TestExpression("1 + true", 2);
			TestExpression("(1 == 0) + 1", 1);
			TestExpression("1 + (true != false)", 2);
		}

		[Test]
		public void Test_Boolean_Subtract()
		{
			TestExpression("true - true", 0);
			TestExpression("true - false", 1);
			TestExpression("(1 == 1) - (2 == 2)", 0);
			TestExpression("(1 == 1) - (2 == 2)", 0);
			TestExpression("(1 != 2) - (1 != 2)", 0);
			TestExpression("true - 1", 0);
			TestExpression("1 - true", 0);
			TestExpression("(1 == 0) - 1", -1);
			TestExpression("1 - (true != false)", 0);
		}

		[Test]
		public void Test_Boolean_Multiply()
		{
			TestExpression("true * true", 1);
			TestExpression("true * false", 0);
			TestExpression("(1 == 1) * (2 == 2)", 1);
			TestExpression("(1 == 1) * (2 == 2)", 1);
			TestExpression("(1 != 2) * (1 != 2)", 1);
			TestExpression("true * 2", 2);
			TestExpression("2 * true", 2);
			TestExpression("(1 == 0) * 2", 0);
			TestExpression("2 * (true != false)", 2);
		}

		[Test]
		public void Test_Boolean_Divide()
		{
			TestExpression("true / true", 1);
			TestExpression("true / false", double.NaN);
			TestExpression("(1 == 1) / (2 == 2)", 1);
			TestExpression("(1 == 1) / (2 == 2)", 1);
			TestExpression("(1 != 2) / (1 != 2)", 1);
			TestExpression("true / 2", 0.5);
			TestExpression("2 / true", 2);
			TestExpression("(1 == 0) / 2", 0);
			TestExpression("2 / (true != false)", 2);
		}

		[Test]
		public void Test_Boolean_Negation()
		{
			TestExpression("!true", false);
			TestExpression("!false", true);
			TestExpression("!!true", true);
			TestExpression("!!false", false);
			TestExpression("!(1 == 0)", true);
			TestExpression("!1", false);
			TestExpression("!0", true);

			TestExpression("! true", false);
			TestExpression("! false", true);
			TestExpression("!! true", true);
			TestExpression("!! false", false);
			TestExpression("! (1 == 0)", true);
			TestExpression("! 1", false);
			TestExpression("! 0", true);

			TestExpression("!!!true", false);
			TestExpression("!!!false", true);
			TestExpression("!!!!true", true);
			TestExpression("!!!!false", false);
			TestExpression("!!!(1 == 0)", true);
			TestExpression("!!!1", false);
			TestExpression("!!!0", true);
		}
		#endregion

		public static void TestExpression(string expr, object expected)
		{
			var expression = new Parser(expr).Parse();

			if (expression == null)
			{
				throw new Exception("Could not parse");
			}

			var resolver = new SymbolRegistry<object>();
			resolver.Add("Spend", DataType.Currency, x => 1);

			// Resolve symbols
			var decorator = new TypeDecoratorVisitor<object>(resolver, true);
			decorator.Visit(expression);

			// Check the types
			var checker = new TypeCheckerVisitor();
			var type = (DataType)(checker.Visit(expression));

			if (type == DataType.Unknown)
			{
				throw new Exception("Could not resolve types");
			}

			// Run it!
			var interpreter = new Evaluator<object>(resolver);
			var result = interpreter.Evaluate(expression);

			Assert.AreEqual(expected, result, expr);
		}
	}
}
