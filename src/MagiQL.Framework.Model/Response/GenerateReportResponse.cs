using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response.Base;

namespace MagiQL.Framework.Model.Response
{
    public class GenerateReportResponse : ResponseBase<ReportStatus>
    {  
        public SearchRequest Request { get; set; }
    }
}