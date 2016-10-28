using System;
using System.Collections.Generic;
using System.Data;
using MagiQL.Framework.Model.Response;
using Newtonsoft.Json;

namespace MagiQL.Framework.Model.Columns
{
    public class ReportColumnMapping : ColumnDefinition
    {
        public ReportColumnMapping()
            : base()
        {
            CalculatedValues = new CalculatedReportColumnMappingValues(); 
        }

        public ReportColumnMapping(ReportColumnMapping source)
            : base(source)
        {
            CalculatedValues = new CalculatedReportColumnMappingValues(); 
        }

        /// <summary>
        /// Which ISearchDataSource this column is used by (implies platform)
        /// </summary>
        public int DataSourceTypeId { get; set; }

        /// <summary>
        /// If Null, the column is available to all organizations
        /// </summary>
        public int? OrganizationId { get; set; }

        public int? CreatedByUserId { get; set; }

        public bool IsPrivate { get; set; }

        /// <summary>
        /// The KnownTable to select the field from
        /// </summary> 
        public string KnownTable { get; set; }

        /// <summary>
        /// The the field or formula to use for the column
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The field or formula to use specifically for lifetime queries (If null, uses FieldName)
        /// </summary>
        public string LifetimeFieldName { get; set; }

        /// <summary>
        /// True unless the field is a field on the table
        /// </summary>
        public bool IsCalculated { get; set; }

        /// <summary>
        /// The method to use when aggregating values
        /// </summary> 
        public FieldAggregationMethod FieldAggregationMethod { get; set; }

        // Needed for ORM to store enums as strings
        [JsonIgnore]
        public string _FieldAggregationMethodString
        {
            get { return FieldAggregationMethod.ToString(); }
            set { FieldAggregationMethod = (FieldAggregationMethod)Enum.Parse(typeof(FieldAggregationMethod), value); }
        }

        /// <summary>
        /// The data type of the column
        /// </summary> 
        public DbType DbType { get; set; }

        // Needed for ORM to store enums as strings
        [JsonIgnore]
        public string _DbTypeString
        {
            get { return DbType.ToString(); }
            set { DbType = (DbType)Enum.Parse(typeof(DbType), value); }
        }

        /// <summary>
        /// If false, will not be returned in the column definitions request
        /// </summary>
        public bool Selectable { get; set; }

        /// <summary>
        /// ID of the ActionSpec to join on -Facebook Specific  right now - will we use this for LI or Tw?
        /// </summary>
        public int? ActionSpecId { get; set; }
          
        /// <summary>
        /// Values which are not stored, but are saved here for cached performance, to save re-calculating
        /// </summary>
        [JsonIgnore]
        public CalculatedReportColumnMappingValues CalculatedValues { get; set; }
        
        /// <summary>
        /// Values which are not stored, but are saved here for cached performance, to save re-calculating
        /// </summary>
        [JsonIgnore]
        public Dictionary<ReportColumnMapping, string> NestedColumns { get; set; }
    }

    // calculated values which are not in the database but are stored on the object for performance
    // this works because the objects are stored in a cache
}