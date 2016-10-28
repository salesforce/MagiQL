namespace MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping
{
    public class TableRelationship
    {
        public TableMapping Table1 { get; set; }
        public string Table1Column { get; set; }

        public TableRelationshipType RelationshipType { get; set; }

        public TableMapping Table2 { get; set; }
        public string Table2Column { get; set; }
        public bool IsDirect { get; set; }
    }
}