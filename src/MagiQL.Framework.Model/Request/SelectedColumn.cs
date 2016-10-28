namespace MagiQL.Framework.Model.Request
{
    public class SelectedColumn
    {
        public SelectedColumn(){}

        public SelectedColumn(int columnId)
        {
            ColumnId = columnId;
        }

        public int ColumnId { get; set; } 
    }
}