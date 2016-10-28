using MagiQL.Expressions.Model;
using NUnit.Framework;

namespace MagiQL.Expressions.Tests.Unit
{
	[TestFixture(Category = "Language")]
	public class TypeCheckerVisitorTests
	{
		#region Literals
		[Test]
		public void TestLiteralNumber()
		{
			var expression = new Parser("1000").Parse();

			Assert.IsInstanceOf<NumberLiteralExpression>(expression);

			var visitor = new TypeCheckerVisitor();
			var type = (DataType)(visitor.Visit(expression));

			Assert.AreEqual(DataType.Number, type);
		}

		[Test]
		public void TestLiteralCurrency()
		{
			var expression = new Parser("$0.5").Parse();

			Assert.IsInstanceOf<CurrencyLiteralExpression>(expression);

			var visitor = new TypeCheckerVisitor();
			var type = (DataType)(visitor.Visit(expression));

			Assert.AreEqual(DataType.Currency, type);
		}

		[Test]
		public void TestLiteralPercent()
		{
			var expression = new Parser("1%").Parse();

			Assert.IsInstanceOf<PercentLiteralExpression>(expression);

			var visitor = new TypeCheckerVisitor();
			var type = (DataType)(visitor.Visit(expression));

			Assert.AreEqual(DataType.Percent, type);
		}
		#endregion

		#region BinaryExpression
		[Test]
		public void TestBinary_Add_Number_Number()
		{
			var expression = new Parser("1 + 1").Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var visitor = new TypeCheckerVisitor();
			var type = (DataType)(visitor.Visit(expression));

			Assert.AreEqual(DataType.Number, type);
		}


		#region Add

		private void TestOp(string str, DataType expected)
		{
			var expression = new Parser(str).Parse();

			Assert.IsInstanceOf<BinaryExpression>(expression);

			var visitor = new TypeCheckerVisitor();
			var type = (DataType)(visitor.Visit(expression));

			Assert.AreEqual(expected, type);
		}

		[Test]
		public void TestBinary_Add()
		{
			TestOp("1+1", DataType.Number);				//Number	+	Number	=	Number
			TestOp("1+$0.5", DataType.Currency);		//Number	+	Currency	=	Currency
			TestOp("1+5%", DataType.Number);			//Number	+	Percent	=	Number
			TestOp("$0.5 + $0.5", DataType.Currency);	//Currency	+	Currency	=	Currency
			TestOp("$0.5 + 1", DataType.Currency);		//Currency	+	Number	=	Currency
			TestOp("$0.5 + 5%", DataType.Currency);		//Currency	+	Percent	=	Currency
			TestOp("5%+5%", DataType.Percent);			//Percent	+	Percent	=	Percent
			TestOp("5% + 5", DataType.Number);			//Percent	+	Number	=	Number
			TestOp("5% + $0.5", DataType.Currency);		//Percent	+	Currency	=	Currency
		}

		[Test]
		public void TestBinary_Subtract()
		{
			//Number	-	Number	=	Number
			//Number	-	Currency	=	Currency
			//Number	-	Percent	=	Number
			//Currency	-	Currency	=	Currency
			//Currency	-	Number	=	Currency
			//Currency	-	Percent	=	Currency
			//Percent	-	Percent	=	Percent
			//Percent	-	Number	=	Number
			//Percent	-	Currency	=	Currency

			TestOp("1-1", DataType.Number);				
			TestOp("1-$0.5", DataType.Currency);		
			TestOp("1-5%", DataType.Number);			
			TestOp("$0.5 - $0.5", DataType.Currency);	
			TestOp("$0.5 - 1", DataType.Currency);		
			TestOp("$0.5 - 5%", DataType.Currency);		
			TestOp("5%-5%", DataType.Percent);			
			TestOp("5% - 5", DataType.Number);			
			TestOp("5% - $0.5", DataType.Currency);		
		}

		[Test]
		public void TestBinary_Multiply()
		{
			//Number	*	Number	=	Number
			//Number	*	Currency	=	Currency
			//Number	*	Percent	=	Number
			//Currency	*	Currency	=	Unknown
			//Currency	*	Number	=	Currency
			//Currency	*	Percent	=	Currency
			//Percent	*	Percent	=	Percent
			//Percent	*	Number	=	Number
			//Percent	*	Currency	=	Currency

			TestOp("1*1", DataType.Number);
			TestOp("1*$0.5", DataType.Currency);
			TestOp("1*5%", DataType.Number);
			TestOp("$0.5 * $0.5", DataType.Currency);
			TestOp("$0.5 * 1", DataType.Currency);
			TestOp("$0.5 * 5%", DataType.Currency);
			TestOp("5%*5%", DataType.Percent);
			TestOp("5% * 5", DataType.Number);
			TestOp("5% * $0.5", DataType.Currency);
		}

		[Test]
		public void TestBinary_Divide()
		{
			//Number	/	Number	=	Number
			//Number	/	Currency	=	Unknown
			//Number	/	Percent	=	Number
			//Currency	/	Currency	=	Number
			//Currency	/	Number	=	Currency
			//Currency	/	Percent	=	Currency
			//Percent	/	Percent	=	Number
			//Percent	/	Number	=	Percent
			//Percent	/	Currency	=	Unknown

			TestOp("1/1", DataType.Number);
			TestOp("1/$0.5", DataType.Unknown);
			TestOp("1/5%", DataType.Number);
			TestOp("$0.5 / $0.5", DataType.Number);
			TestOp("$0.5 / 1", DataType.Currency);
			TestOp("$0.5 / 5%", DataType.Currency);
			TestOp("5%/5%", DataType.Number);
			TestOp("5% / 5", DataType.Percent);
			TestOp("5% / $0.5", DataType.Unknown);
		}
		#endregion
		#endregion
	}
}
