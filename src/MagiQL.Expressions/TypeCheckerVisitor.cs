using MagiQL.Expressions.Model;

namespace MagiQL.Expressions
{
	public class TypeCheckerVisitor : Visitor
	{
		public override object Visit(BinaryExpression ex)
		{
			var leftType = (DataType)ex.Left.Visit(this);
			var rightType = (DataType)ex.Right.Visit(this);

			var result = ConvertType(leftType, rightType, ex.Operator);
			ex.DataType = result;

			return result;
		}
		
		public override object Visit(NumberLiteralExpression ex)
		{
			return DataType.Number;
		}

		public override object Visit(PercentLiteralExpression ex)
		{
			return DataType.Percent;
		}

		public override object Visit(CurrencyLiteralExpression ex)
		{
			return DataType.Currency;
		}

		public override object Visit(BooleanLiteralExpression ex)
		{
			return DataType.Boolean;
		}

		public override object Visit(IdentifierExpression ex)
		{
			return ex.DataType;
		}

		public override object Visit(UnaryExpression ex)
		{
			var type = (DataType)ex.Expression.Visit(this);
			ex.DataType = type;

			return type;
		}

		#region Type Conversion
		private static DataType ConvertType(DataType leftType, DataType rightType, Operator op)
		{
			if (leftType == DataType.Unknown || rightType == DataType.Unknown)
			{
				return DataType.Unknown;
			}

			switch (op)
			{
				case Operator.Add:
					return ConvertType_Add(leftType, rightType);
				case Operator.Subtract:
					return ConvertType_Subtract(leftType, rightType);
				case Operator.Multiply:
					return ConvertType_Multiply(leftType, rightType);
				case Operator.Divide:
					return ConvertType_Divide(leftType, rightType);

				case Operator.Equals:
				case Operator.NotEquals:
				case Operator.GreaterThan:
				case Operator.GreaterThanEqualTo:
				case Operator.LessThan:
				case Operator.LessThanEqualTo:
				case Operator.LogicalAnd:
				case Operator.LogicalOr:
					return DataType.Boolean;

				default:
					return DataType.Unknown;
			}
		}

		private static DataType ConvertType_Add(DataType leftType, DataType rightType)
		{
			//Number	+	Number	=	Number
			//Number	+	Currency	=	Currency
			//Number	+	Percent	=	Number
			//Currency	+	Currency	=	Currency
			//Currency	+	Number	=	Currency
			//Currency	+	Percent	=	Currency
			//Percent	+	Percent	=	Percent
			//Percent	+	Number	=	Number
			//Percent	+	Currency	=	Currency

			if (leftType == DataType.Number)
			{
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Currency)
			{
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Number) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Currency;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Percent)
			{
				if (rightType == DataType.Percent) return DataType.Percent;
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Boolean)
			{
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Boolean) return DataType.Number;
			}

			return DataType.Unknown;
		}

		private static DataType ConvertType_Subtract(DataType leftType, DataType rightType)
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

			if (leftType == DataType.Number)
			{
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Currency)
			{
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Number) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Currency;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Percent)
			{
				if (rightType == DataType.Percent) return DataType.Percent;
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Boolean)
			{
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Boolean) return DataType.Number;
			}

			return DataType.Unknown;
		}

		private static DataType ConvertType_Multiply(DataType leftType, DataType rightType)
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

			if (leftType == DataType.Number)
			{
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Currency)
			{
				if (rightType == DataType.Currency) return DataType.Currency;	// BUT WHY!?
				if (rightType == DataType.Number) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Currency;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Percent)
			{
				if (rightType == DataType.Percent) return DataType.Percent;
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Boolean)
			{
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Boolean) return DataType.Number;
			}

			return DataType.Unknown;
		}

		private static DataType ConvertType_Divide(DataType leftType, DataType rightType)
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

			if (leftType == DataType.Number)
			{
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Unknown;
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Currency)
			{
				if (rightType == DataType.Currency) return DataType.Number;
				if (rightType == DataType.Number) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Currency;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Percent)
			{
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Number) return DataType.Percent;
				if (rightType == DataType.Currency) return DataType.Unknown;
				if (rightType == DataType.Boolean) return DataType.Number;
			}
			else if (leftType == DataType.Boolean)
			{
				if (rightType == DataType.Number) return DataType.Number;
				if (rightType == DataType.Currency) return DataType.Currency;
				if (rightType == DataType.Percent) return DataType.Number;
				if (rightType == DataType.Boolean) return DataType.Number;
			}

			return DataType.Unknown;
		}
		#endregion
	}
}
