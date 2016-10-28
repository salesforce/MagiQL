namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class ToBoolFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "TOBOOL"; }
        }

        protected override int ArgumentCount
        {
            get { return 1; }
        }

        protected override string FunctionFormat
        {
            get { return "(CASE WHEN {0} THEN 1 ELSE 0 END)"; }
        }
    }
}