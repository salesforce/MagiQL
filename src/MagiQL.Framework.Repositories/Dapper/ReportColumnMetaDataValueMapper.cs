using DapperExtensions.Mapper;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Repositories.Dapper
{
    public class ReportColumnMetaDataValueMapper : ClassMapper<ReportColumnMetaDataValue>
    {
        public ReportColumnMetaDataValueMapper()
        {
            Table("ReportColumnMappingMetaData");
            Map(x => x.Id).Column("ID").Key(KeyType.Identity); 

            AutoMap();
        }
    }
}