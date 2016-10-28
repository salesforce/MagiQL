namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class IsNullFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "ISNULL"; }
        }

        protected override int ArgumentCount
        {
            get { return 2; }
        }
         
        protected override string FunctionFormat
        {
            get { return "ISNULL({0},{1})"; }
        }
    }
}
