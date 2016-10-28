namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{ 
    public class MaxOfFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "MAXOF"; }
        }

        protected override int ArgumentCount
        {
            get { return 2; }
        }
         
        protected override string FunctionFormat
        {
            get { return "(CASE WHEN {0} > {1} THEN {0} ELSE {1} END)"; }
        }
    }
}
