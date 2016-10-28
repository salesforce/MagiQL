namespace MagiQL.Expressions.Model
{
	public enum Operator
	{
		None = 1,
		Add = 2,
		Subtract = 3,
		Multiply = 4,
		Divide = 5,
		Minus = 6,	// Unary - not the same as subtraction

		GreaterThan = 7,
		LessThan = 8,
		GreaterThanEqualTo = 9,
		LessThanEqualTo = 10,
		LogicalAnd = 11,
		LogicalOr = 12,
		Negate = 13,
		Equals = 14,
		NotEquals = 15,
	}
}
