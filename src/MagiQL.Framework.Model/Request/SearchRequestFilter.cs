using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MagiQL.Framework.Model.Request
{ 
    [DataContract]
    public class SearchRequestFilter
    {
        [DataMember]
        public int? ColumnId { get; set; }

        /// <summary>
        /// One or more values to match on, multiple values are treated as an OR
        /// </summary>
        [DataMember]
        public List<string> Values { get; set; } 

        // todo:remove me
        [Obsolete]
        public string Value { set {Values = new List<string>{value};} }

        [DataMember]
        public bool? Exclude { get; set; }

        [DataMember]
        public FilterModeEnum? Mode { get; set; }

        public bool ProcessBeforeAggregation { get; set; }
    
    }
}