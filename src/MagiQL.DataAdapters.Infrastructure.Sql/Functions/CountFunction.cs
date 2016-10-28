namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class CountFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "COUNT"; }
        }

        protected override int ArgumentCount
        {
            get { return 0; }
        }

        protected override string FunctionFormat
        {
            get { return "_C"; }
        }
    }
}