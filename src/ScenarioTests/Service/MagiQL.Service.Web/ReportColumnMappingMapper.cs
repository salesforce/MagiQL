using BrighterOption.Reports.Framework.Model.Columns;
using DapperExtensions.Mapper;

namespace BrighterOption.Reports.Service.Web
{
    public class ReportColumnMappingMapper : ClassMapper<ReportColumnMapping>
    {
        public ReportColumnMappingMapper()
        {

            //use a custom schema
            //Schema("dto");

            Table("ReportColumnMapping");
//            Map(x => x.Id).Key(KeyType.Assigned);
//            Map(x => x.CanGroupBy).Ignore();
//            Map(x => x.DbType).Ignore();
//            Map(x => x._DbTypeString).Column("DbType");
//            Map(x => x.FieldAggregationMethod).Ignore();
//            Map(x => x._FieldAggregationMethodString).Column("FieldAggregationMethod");
//            Map(x => x.KnownTable).Column("KnownTable");
//            Map(x => x.CalculatedValues).Ignore();
//            Map(x => x.IsStat).Ignore();
//
//            // temp
//            Map(x => x.MetaData).Ignore();

            //HasMany(x => x.MetaData).WithRequired(x => x.ReportColumnMapping).Map(m =>
            //{
            //    m.ToTable("ReportColumnMappingMetaData");
            //    m.MapKey("ReportColumnMappingID");
            //}).WillCascadeOnDelete(true);


            //optional, map all other columns
            //AutoMap();
        }
    }
}