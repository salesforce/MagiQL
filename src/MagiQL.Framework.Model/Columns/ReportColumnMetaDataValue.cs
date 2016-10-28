using Newtonsoft.Json;

namespace MagiQL.Framework.Model.Columns
{
    public class ReportColumnMetaDataValue
    {
        [JsonIgnore] 
        public long Id { get; set; }

        [JsonIgnore]
        public int ReportColumnMappingId { get; set; }
  
        public string Name { get; set; }

        public string Value { get; set; }
    }
}