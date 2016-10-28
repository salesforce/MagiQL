using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Model
{
    public class MappedSearchRequestFilter
    {
        public ReportColumnMapping Column { get; set; }

        public List<string> Values { get; set; }

        public bool? Exclude { get; set; }

        public FilterModeEnum? Mode { get; set; }

        /// <summary>
        /// Set internally by the report engine, true if a field is a non aggregated column
        /// </summary> 
        public bool ProcessBeforeAggregation { get; set; }
    }
}