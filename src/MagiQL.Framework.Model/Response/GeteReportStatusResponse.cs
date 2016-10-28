using MagiQL.Framework.Model.Response.Base;

namespace MagiQL.Framework.Model.Response
{
    
    public class GetReportStatusResponse : ResponseBase<ReportStatus>
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }
}