namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class CeilingFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "CEILING"; }
        }

        protected override int ArgumentCount
        {
            get { return 1; }
        }
         
        protected override string FunctionFormat
        {
            get { return "CEILING({0})"; }
        }
    }
}
