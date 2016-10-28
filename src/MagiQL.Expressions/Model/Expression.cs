namespace MagiQL.Expressions.Model
{
	public abstract class Expression
	{
		public virtual DataType DataType { get; set; }

		public abstract object Visit(Visitor visitor);
	}
}
