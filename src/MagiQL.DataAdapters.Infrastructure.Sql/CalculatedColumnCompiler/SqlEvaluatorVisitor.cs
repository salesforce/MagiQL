using MagiQL.Expressions;
using MagiQL.Expressions.Model;

namespace MagiQL.DataAdapters.Infrastructure.Sql.CalculatedColumnCompiler
{
    public class SqlEvaluatorVisitor : Visitor
    {  
        public override object Visit(BinaryExpression ex)
        {
            var left = ex.Left.Visit(this);
            var right = ex.Right.Visit(this);
            object result = null;

            switch (ex.Operator)
            {
                case Operator.Add:
                    result = Operator_Add(ex.Left, ex.Right, left, right);
                    break;
                case Operator.Divide:
                    result = Operator_Divide(ex.Left, ex.Right, left, right);
                    break;
                case Operator.Multiply:
                    result = Operator_Multiply(ex.Left, ex.Right, left, right);
                    break;
                case Operator.Subtract:
                    result = Operator_Subtract(ex.Left, ex.Right, left, right);
                    break;

                case Operator.Equals:
                case Operator.NotEquals:
                case Operator.GreaterThan:
                case Operator.GreaterThanEqualTo:
                case Operator.LessThan:
                case Operator.LessThanEqualTo:
                    result = Operator_Comparison(ex.Operator, ex.Left, ex.Right, left, right);
                    break;

                case Operator.LogicalAnd:
                case Operator.LogicalOr:
                    //var leftBool = CastBoolean(ex.Left, left);
                    //var rightBool = CastBoolean(ex.Right, right);
                    //result = Operator_Logical(ex.Operator, leftBool, rightBool);
                    result = Operator_Logical(ex.Operator, ex.Left, ex.Right, left, right);
                    break;
            }

            return result;
        }

        public override object Visit(UnaryExpression ex)
        {
            object val = ex.Expression.Visit(this);

            switch (ex.Operator)
            {
                case Operator.Minus:
                    return -CastNumber(ex, val);
                case Operator.Negate:
                    return !CastBoolean(ex, val);
            }

            throw new ExpressionException("Unknown unary operator '" + ex.Operator.ToString() + "'");
        }

        public override object Visit(IdentifierExpression ex)
        {
            return ex.Identifier; 
        }

        public override object Visit(NumberLiteralExpression ex)
        {
            return ex.Value;
        }

        public override object Visit(PercentLiteralExpression ex)
        {
            return ex.Value;
        }

        public override object Visit(CurrencyLiteralExpression ex)
        {
            return ex.Value;
        }

        public override object Visit(BooleanLiteralExpression ex)
        {
            return ex.Value;
        }

        private string Operator_Add(Expression left, Expression right, object leftValue, object rightValue)
        { 
            // Special cases:
            //		- Adding a % to a number or currency

            if ((left.DataType == DataType.Percent || right.DataType == DataType.Percent) && left.DataType != right.DataType)
            {
                string percentValue;
                string nonPercent;

                if (left.DataType == DataType.Percent)
                {
                    percentValue = leftValue.ToString();
                    nonPercent = rightValue.ToString();
                }
                else
                {
                    percentValue = rightValue.ToString();
                    nonPercent = leftValue.ToString();
                }

                // So if we say 100 + 10%, what we're really saying is (100 + (100 * 10%)) = 110
                return string.Format("(ISNULL({0},0) + (ISNULL({0},0) * {1}))", nonPercent, percentValue);
            }

            return string.Format("(ISNULL({0},0) + ISNULL({1},0))", leftValue, rightValue);
        }

        private string Operator_Subtract(Expression left, Expression right, object leftValue, object rightValue)
        { 
            // Special cases:
            //		- Subtracting a % to a number or currency

            if ((left.DataType == DataType.Percent || right.DataType == DataType.Percent) && left.DataType != right.DataType)
            {
                string percentValue;
                string nonPercent;

                if (left.DataType == DataType.Percent)
                {
                    percentValue = leftValue.ToString();
                    nonPercent = rightValue.ToString();
                }
                else
                {
                    percentValue = rightValue.ToString();
                    nonPercent = leftValue.ToString();
                }

                // So if we say 100 - 10%, what we're really saying is (100 - (100 * 10%)) = 110
                return string.Format("(ISNULL({0},0) - (ISNULL({0},0) * {1}))", nonPercent, percentValue);
            }

            return string.Format("(ISNULL({0},0) - ISNULL({1},0))", leftValue, rightValue);
        }

        private string Operator_Multiply(Expression left, Expression right, object leftValue, object rightValue)
        { 
            return string.Format("({0} * {1})", leftValue, rightValue);
        }

        private string Operator_Divide(Expression left, Expression right, object leftValue, object rightValue)
        {
            return string.Format("((1.0 * {0}) / NULLIF({1}, 0))", leftValue, rightValue);
        }

        private string Operator_Logical(Operator op, Expression left, Expression right, object leftValue, object rightValue)
        {
            switch (op)
            {
                case Operator.LogicalAnd:
                    return string.Format("(({0}) AND ({1}))", leftValue, rightValue);
                case Operator.LogicalOr:
                    return string.Format("(({0}) OR ({1}))", leftValue, rightValue);
                default:
                    throw new ExpressionException("Unknown logical operator '" + op.ToString() + "'");
            }
        }
         
        private string Operator_Comparison(Operator op, Expression left, Expression right, object leftValue, object rightValue)
        {
            

            switch (op)
            {
                case Operator.Equals:
                    return string.Format("{0} = {1}", leftValue, rightValue);
                case Operator.LessThan:
                    return string.Format("{0} < {1}", leftValue, rightValue);
                case Operator.LessThanEqualTo:
                    return string.Format("{0} <= {1}", leftValue, rightValue);
                case Operator.GreaterThan:
                    return string.Format("{0} > {1}", leftValue, rightValue);
                case Operator.GreaterThanEqualTo:
                    return string.Format("{0} >= {1}", leftValue, rightValue);
                case Operator.NotEquals:
                    return string.Format("{0} != {1}", leftValue, rightValue);
                default:
                    throw new ExpressionException("Unknown comparison operator '" + op.ToString() + "'");
            }
        }

        private double CastNumber(Expression expression, object value)
        {
            if (value is double)
            {
                return (double)value;
            }
            else if (value is bool)
            {
                return ((bool)value) ? 1 : 0;
            }

            return double.NaN;
        }

        private bool CastBoolean(Expression expression, object value)
        {
            if (value is double)
            {
                return (double)value != 0;
            }
            else if (value is bool)
            {
                return (bool)value;
            }

            return false;
        }
    }
}