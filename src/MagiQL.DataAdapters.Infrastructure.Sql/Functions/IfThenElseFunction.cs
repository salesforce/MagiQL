namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class IfThenElseFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "IFTHENELSE"; }
        }

        protected override int ArgumentCount
        {
            get { return 3; }
        }

        protected override string FunctionFormat
        {
            get { return "(CASE WHEN {0} THEN {1} ELSE {2} END)"; }
        }
    }
}