using DapperExtensions.Mapper;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Repositories.Dapper
{
    public class ReportColumnMappingMapper : ClassMapper<ReportColumnMapping>
    {
        public ReportColumnMappingMapper()
        { 
            Table("ReportColumnMapping");
            Map(x => x.Id).Key(KeyType.Identity);
            Map(x => x.CanGroupBy).Ignore();
            Map(x => x.DbType).Ignore();
            Map(x => x._DbTypeString).Column("DbType");
            Map(x => x.FieldAggregationMethod).Ignore();
            Map(x => x._FieldAggregationMethodString).Column("FieldAggregationMethod");
            Map(x => x.KnownTable).Column("KnownTable");
            Map(x => x.CalculatedValues).Ignore();
            Map(x => x.IsStat).Ignore();
            Map(x => x.NestedColumns).Ignore();

            // children
            Map(x => x.MetaData).Ignore();

            AutoMap();
        }
    }
}