using MagiQL.Framework.Interfaces.Repository;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Services
{
    public class ReportColumnMappingCreationService : IReportColumnMappingCreationService
    {
        private readonly IReportColumnMappingRepository _reportColumnMappingRepository;
        private readonly IColumnProviderCacheService _columnProviderCacheService;
        private readonly IReportColumnMappingQueryService _reportColumnMappingQueryService;

        public ReportColumnMappingCreationService(
            IReportColumnMappingRepository reportColumnMappingRepository,
            IColumnProviderCacheService columnProviderCacheService,
            IReportColumnMappingQueryService reportColumnMappingQueryService
            )
        { 
            this._reportColumnMappingRepository = reportColumnMappingRepository;
            _columnProviderCacheService = columnProviderCacheService;
            _reportColumnMappingQueryService = reportColumnMappingQueryService;
        } 

        public void InsertReportColumnMapping(ReportColumnMapping value)
        {
            using (var scope = _reportColumnMappingRepository.CreateTransaction())
            {
                _reportColumnMappingRepository.Add(value, scope);
                scope.Commit();
            }

            // reload mappings
            _columnProviderCacheService.ClearMappings(value.DataSourceTypeId, value.OrganizationId);
            _reportColumnMappingQueryService.GetReportColumnMappingsByOrganizationId(value.DataSourceTypeId, value.OrganizationId); // to ensure that all columns are reloaded
        }
    }
}