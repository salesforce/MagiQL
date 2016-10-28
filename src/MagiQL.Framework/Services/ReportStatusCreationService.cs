using MagiQL.Framework.Interfaces.Repository;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Services
{
    public class ReportStatusCreationService : IReportStatusCreationService
    { 
        private readonly IReportStatusRepository _reportStatusRepository;

        public ReportStatusCreationService(IReportStatusRepository reportStatusRepository)
        {
            _reportStatusRepository = reportStatusRepository;
        }

        public void InsertReportStatus(ReportStatus value)
        {
            value.Id = 0;
            using (var scope = _reportStatusRepository.CreateTransaction())
            { 
                _reportStatusRepository.Add(value, scope);
                scope.Commit();
            }
        }
    }
}