using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Model.Response
{
    /// <summary>
    /// Information used to select and display available columns
    /// </summary> 
    public class ColumnDefinition
    { 

        public ColumnDefinition()
        {
            MetaData = new List<ReportColumnMetaDataValue>();
        }

        public ColumnDefinition(ColumnDefinition source)
        { 
            this.Id = source.Id;
            this.UniqueName = source.UniqueName;
            this.DisplayName = source.DisplayName;
            this.CanGroupBy = source.CanGroupBy;
            this.MainCategory = source.MainCategory;
            this.SubCategory = source.SubCategory;
            this.IsStat = source.IsStat;
            this.MetaData = source.MetaData ?? new List<ReportColumnMetaDataValue>();
        } 

        /// <summary>
        /// The unique identifier used to select the column in a request
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The unique name of the column used when building custom columns
        /// </summary> 
        public string UniqueName { get; set; }

        /// <summary>
        /// The name of the column to be used in the UI / Reports
        /// </summary> 
        public string DisplayName { get; set; }

        /// <summary>
        /// Describes what the column is used for
        /// </summary> 
        public string Description { get; set; }
            
        /// <summary>
        /// Is grouping by this column supported
        /// </summary> 
        public bool CanGroupBy { get; set; }

        /// <summary>
        /// The top level category for this column
        /// </summary> 
        public string MainCategory { get; set; }

        /// <summary>
        /// The second level category for this column
        /// </summary> 
        public string SubCategory { get; set; }

        /// <summary>
        /// Determines whether this is a stat
        /// </summary>
        public virtual bool IsStat { get; set; }
        
        public virtual ICollection<ReportColumnMetaDataValue> MetaData { get; set; }
        
  
    }
}