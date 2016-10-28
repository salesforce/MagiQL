using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Services
{
    public class ManagedColumnProvider : ColumnProviderBase
    {
        private readonly IReportColumnMappingQueryService _reportColumnMappingQueryService; 
        private readonly IReportColumnMappingUpdaterService _reportColumnMappingUpdaterService;
        private readonly IReportColumnMappingCreationService _reportColumnMappingCreationService; 
        private readonly IColumnProviderCacheService _columnProviderCacheService;

        public ManagedColumnProvider(
            IReportColumnMappingQueryService reportColumnMappingQueryService, 
            IReportColumnMappingUpdaterService reportColumnMappingUpdaterService, 
            IReportColumnMappingCreationService reportColumnMappingCreationService, 
            IColumnProviderCacheService columnProviderCacheService)
        {
            _reportColumnMappingQueryService = reportColumnMappingQueryService;
            _reportColumnMappingUpdaterService = reportColumnMappingUpdaterService;
            _reportColumnMappingCreationService = reportColumnMappingCreationService; 
            _columnProviderCacheService = columnProviderCacheService;
        }

        public override ReportColumnMapping GetColumnMapping(int dataSourceId, int id)
        {
            return GetColumnMappings(dataSourceId, new [] {id}).FirstOrDefault();
        } 

        public override List<ReportColumnMapping> GetColumnMappings(int dataSourceId, IEnumerable<int> columnIds)
        {
            var result = _reportColumnMappingQueryService.GetReportColumnMappingsById(dataSourceId, columnIds).ToList();
            FixValues(result);
            return result;
        }

        public override List<ReportColumnMapping> GetAllColumnMappings(int dataSourceId, int? organizationId)
        {
            var result = _reportColumnMappingQueryService.GetReportColumnMappingsByOrganizationId(dataSourceId, organizationId).ToList();
            FixValues(result);
            return result;
        } 

        public override List<ReportColumnMapping> Find(int dataSourceId, string table, string field, int? statTransposeKeyValue, bool cacheOnly = false)
        {
            var result =  _reportColumnMappingQueryService.Find(dataSourceId, table, field, statTransposeKeyValue, cacheOnly).ToList();
            FixValues(result);
            return result;
        }

        public override List<ReportColumnMapping> Find(int dataSourceId, string uniqueName, bool cacheOnly = true)
        {
            var result = _reportColumnMappingQueryService.Find(dataSourceId, uniqueName, cacheOnly).ToList();
            FixValues(result);
            return result;
        }

        public override ReportColumnMapping UpdateColumnMapping(int dataSourceId, int columnId, ReportColumnMapping columnMapping, IReportColumnMappingValidator columnValidator)
        {
            if (columnMapping.Id != columnId)
            {
                throw new Exception("Column ID does not match");
            }

            if (!columnValidator.FieldNameIsValid(columnMapping.FieldName))
            {
                throw new Exception("Column FieldName is invalid");
            }

            _reportColumnMappingUpdaterService.UpdateReportColumnMapping(columnMapping);

            return _reportColumnMappingQueryService.GetReportColumnMappingsById(dataSourceId, new[] {columnId}).FirstOrDefault();
        }

        public override void ClearCache(int dataSourceTypeId)
        {
            _columnProviderCacheService.ClearMappings(dataSourceTypeId, organizationId: null);   
        }

        public override ReportColumnMapping CreateColumnMapping(int dataSourceId, ReportColumnMapping columnMapping, IReportColumnMappingValidator columnValidator)
        {
            if (!columnValidator.FieldNameIsValid(columnMapping.FieldName))
            {
                throw new Exception("Column FieldName is invalid");
            }

            columnMapping.DataSourceTypeId = dataSourceId;
            _reportColumnMappingCreationService.InsertReportColumnMapping(columnMapping);
            return _reportColumnMappingQueryService.GetReportColumnMappingsById(dataSourceId, new[] { columnMapping.Id }).FirstOrDefault();
        }

        private void FixValues(IEnumerable<ReportColumnMapping> result)
        {
            foreach (var r in result)
            {
                r.CanGroupBy = !r.KnownTable.Contains("Stats"); // todo : this needs to be FB/LI/TW specific
                r.IsStat = r.KnownTable.Contains("Stats");
            }
        }

        protected override bool IsSelectable(ReportColumnMapping column, ReportColumnMapping groupBy)
        {
            return column.Selectable; 
        }

        public override void Start()
        { 
        }
    }
}
