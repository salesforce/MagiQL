using DapperExtensions.Mapper;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Repositories.Dapper
{
    public class ReportStatusMapper : ClassMapper<ReportStatus>
    {
        public ReportStatusMapper()
        {
            Table("ReportStatus");
            Map(x => x.Id).Key(KeyType.Identity);
            Map(x => x.Duration).Ignore();

            AutoMap();
        }
    }
}