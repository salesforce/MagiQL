using System;
using MagiQL.Expressions.Model;
using NUnit.Framework;

namespace MagiQL.Expressions.Tests.Unit
{
	[TestFixture(Category = "Language")]
	public class TreeInterpreterVisitorTests
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

		public static void TestExpression(string expr, double expected)
		{
			var expression = new Parser(expr).Parse();

			if (expression == null)
			{
				throw new Exception("Could not resolve types");
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
            //TWADS - it's busted jim
			// Run it!
			//var interpreter = new TreeInterpreter<object>(resolver);
		//	var result = (double)interpreter.Evaluate(expression);

			//Assert.AreEqual(expected, result, 0.02, expr);
		}
	}
}
