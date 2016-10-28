namespace MagiQL.DataExplorer.Web.Controllers
{
    public class ColumnUpdatePostModel 
    { 
        public int Id { get; set; }
         
        public string UniqueName { get; set; }
         
        public string DisplayName { get; set; }
         
        public string Description { get; set; }
         
        public bool CanGroupBy { get; set; }
         
        public string MainCategory { get; set; }
         
        public string SubCategory { get; set; }
         
        public virtual bool IsStat { get; set; }
          
        public int DataSourceTypeId { get; set; }
         
        public int? OrganizationId { get; set; }

        public int? CreatedByUserId { get; set; }

        public bool IsPrivate { get; set; }
         
        public string KnownTable { get; set; }
         
        public string FieldName { get; set; }
         
        public string LifetimeFieldName { get; set; }
         
        public bool IsCalculated { get; set; }
 
        public string _FieldAggregationMethodString { get; set; }
          
        public string _DbTypeString { get; set; }

        public bool Selectable { get; set; }

        public int? ActionSpecId { get; set; }
         
        public string MetaData { get; set; }
    }
}
