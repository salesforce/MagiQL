using MagiQL.Framework.Interfaces.Repository;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Framework.Services
{
    public class ReportStatusUpdaterService : IReportStatusUpdaterService
    { 
        private readonly IReportStatusRepository _reportStatusRepository;

        public ReportStatusUpdaterService(IReportStatusRepository reportStatusRepository)
        {
            _reportStatusRepository = reportStatusRepository;
        }

        public void UpdateReportStatus(ReportStatus value)
        {
            using (var scope = _reportStatusRepository.CreateTransaction())
            {
                var existing = _reportStatusRepository.GetReportStatus(value.Id);

                existing.DateUpdated = value.DateUpdated;
                existing.DateCompleted = value.DateCompleted;
                existing.StatusMessage = value.StatusMessage;
                existing.ProgressPercentage = value.ProgressPercentage;
                existing.ErrorMessage = value.ErrorMessage;
                existing.StackTrace = value.StackTrace;

                _reportStatusRepository.Update(existing, scope);

                scope.Commit();
            }
        }
         
    }
}