using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping
{
    // important that this class is sealed as its used to determine the right table for stats
    public sealed class StatsTableMapping : TableMapping
    {
        public List<StatsTableMappingResolutionTable> ResolutionTables { get; set; }

        public StatsTableMapping()
        {
            this.TableType= TableType.Stats;
            this.Alias = "Stats";
        }
    }

    public sealed class TransposeStatsTableMapping : TableMapping
    {
        public string TransposeKey { get; set; }

        public List<StatsTableMappingResolutionTable> ResolutionTables { get; set; }

        public TransposeStatsTableMapping()
        {
            this.TableType= TableType.Stats;
        }
    }
}