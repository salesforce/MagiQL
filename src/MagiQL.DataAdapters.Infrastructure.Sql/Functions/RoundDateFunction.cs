namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class RoundDateFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "ROUNDDATE"; }
        }

        protected override int ArgumentCount
        {
            get { return 2; }
        }
         
        protected override string FunctionFormat
        {
            get { return "DATEADD({0}, DATEDIFF({0}, 0, {1}), 0)"; }
        }
    }
}
