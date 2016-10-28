using MagiQL.Framework.Model;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Model.TableMapping
{
    public class StatsTableMappingResolutionTable
    {
        public string JoinTable { get; set; }

        public TemporalAggregation TemporalAggregation { get; set; }

        public string DbTableName { get; set; }
    }
}