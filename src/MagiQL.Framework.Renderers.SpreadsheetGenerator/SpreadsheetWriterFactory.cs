namespace MagiQL.Framework.Renderers.SpreadsheetGenerator
{
    public class SpreadsheetWriterFactory : ISpreadsheetWriterFactory
    {
        public ISpreadsheetWriter NewWriter()
        {
            return new SpreadsheetWriter();
        }

    }
}
