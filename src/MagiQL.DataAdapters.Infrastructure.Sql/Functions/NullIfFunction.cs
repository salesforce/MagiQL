namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class NullIfFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "NULLIF"; }
        }

        protected override int ArgumentCount
        {
            get { return 2; }
        }

        protected override string FunctionFormat
        {
            get { return "NULLIF({0},{1})"; }
        }
    }
}