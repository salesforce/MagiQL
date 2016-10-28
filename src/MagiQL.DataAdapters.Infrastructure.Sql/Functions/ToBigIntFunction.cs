namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class ToBigIntFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "TOBIGINT"; }
        }

        protected override int ArgumentCount
        {
            get { return 1; }
        }

        protected override string FunctionFormat
        {
            get { return "CAST({0} AS BIGINT)"; }
        }
    }
}