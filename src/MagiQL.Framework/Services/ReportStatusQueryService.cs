using MagiQL.Framework.Interfaces.Repository;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Services
{
    public class ReportStatusQueryService : IReportStatusQueryService
    { 
        private readonly IReportStatusRepository _reportStatusRepository;

        public ReportStatusQueryService(IReportStatusRepository reportStatusRepository)
        { 
            this._reportStatusRepository = reportStatusRepository;

        } 

        public ReportStatus GetReportStatus(long id)
        {
            //using (_dbContextScopeFactory.CreateReadOnly())
            {
                var result = _reportStatusRepository.GetReportStatus(id);
                 
                return result;
            }
        }

     }
}