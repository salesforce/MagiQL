namespace MagiQL.Framework.Renderers.SpreadsheetGenerator
{
    public interface ISpreadsheetWriterFactory
    {
        ISpreadsheetWriter NewWriter();
    }
}
