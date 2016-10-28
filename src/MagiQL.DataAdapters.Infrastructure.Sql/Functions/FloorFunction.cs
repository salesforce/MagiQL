namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public class FloorFunction : InlineSqlFunction
    {
        public override string Name
        {
            get { return "FLOOR"; }
        }

        protected override int ArgumentCount
        {
            get { return 1; }
        }
         
        protected override string FunctionFormat
        {
            get { return "FLOOR({0})"; }
        }
    }
}
