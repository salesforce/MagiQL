using MagiQL.Framework.Interfaces.Repository;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Services
{
    public class ReportColumnMappingUpdaterService : IReportColumnMappingUpdaterService
    { 
        private readonly IReportColumnMappingRepository _reportColumnMappingRepository;
        private readonly IColumnProviderCacheService _columnProviderCacheService;
        private readonly IReportColumnMappingQueryService _reportColumnMappingQueryService;
       

        public ReportColumnMappingUpdaterService(
            IReportColumnMappingRepository reportColumnMappingRepository, 
            IColumnProviderCacheService columnProviderCacheService,
            IReportColumnMappingQueryService reportColumnMappingQueryService
            )
        { 
            this._reportColumnMappingRepository = reportColumnMappingRepository;
            _columnProviderCacheService = columnProviderCacheService;
            _reportColumnMappingQueryService = reportColumnMappingQueryService;
        }

        public void UpdateReportColumnMapping(ReportColumnMapping value)
        {
            value.CalculatedValues.Clear();

            using (var scope = _reportColumnMappingRepository.CreateTransaction())
            {
                var entity = _reportColumnMappingRepository.GetReportColumnMapping(value.Id);
 
                entity.ActionSpecId = value.ActionSpecId;
                entity.CanGroupBy = value.CanGroupBy;
                entity.DataSourceTypeId = value.DataSourceTypeId;
                entity.DbType = value.DbType;
                entity.DisplayName = value.DisplayName;
                entity.FieldAggregationMethod = value.FieldAggregationMethod;
                entity.FieldName = value.FieldName;
                entity.IsCalculated = value.IsCalculated;
                entity.IsPrivate = value.IsPrivate;
                entity.KnownTable = value.KnownTable;
                entity.LifetimeFieldName = value.LifetimeFieldName;
                entity.MainCategory = value.MainCategory;
                entity.OrganizationId = value.OrganizationId;
                entity.Selectable = value.Selectable;
                entity.SubCategory = value.SubCategory;
                entity.UniqueName = value.UniqueName;
                
                UpdateMetaData(entity, value);

                _reportColumnMappingRepository.Update(entity, scope);

                scope.Commit();
            }

            // reload mappings
            _columnProviderCacheService.ClearMappings(value.DataSourceTypeId, value.OrganizationId);
            _reportColumnMappingQueryService.GetReportColumnMappingsByOrganizationId(value.DataSourceTypeId, value.OrganizationId); // to ensure that all columns are reloaded
        }

        private void UpdateMetaData(ReportColumnMapping entity, ReportColumnMapping value)
        { 
            entity.MetaData.Clear();
            
            foreach (var update in value.MetaData)
            { 
                entity.MetaData.Add(new ReportColumnMetaDataValue()
                {
                    Name = update.Name,
                    Value = update.Value
                });
            }
        }
    }
}