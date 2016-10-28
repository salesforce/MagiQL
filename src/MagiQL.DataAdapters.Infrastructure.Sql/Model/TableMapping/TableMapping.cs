using MagiQL.Framework.Model.Columns;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping
{
    public class TableMapping
    {
        public string KnownTableName { get; set; }
        public string DbTableName { get; set; }
        public string Alias { get; set; } 
        public string PrimaryKey { get; set; }
        public string CteAlias { get { return Alias + "Cte"; } }
        public TableType TableType { get; set; }

        public TableMapping()
        {
            this.TableType = TableType.Data;
        }
    }
}