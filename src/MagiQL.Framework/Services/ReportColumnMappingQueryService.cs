using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MagiQL.Framework.Interfaces.Repository;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Services
{
    public class ReportColumnMappingQueryService : IReportColumnMappingQueryService
    { 
        private readonly IReportColumnMappingRepository _reportColumnMappingRepository;
        private readonly IColumnProviderCacheService _columnProviderCacheService;

        public ReportColumnMappingQueryService(
            IReportColumnMappingRepository reportColumnMappingRepository,
            IColumnProviderCacheService columnProviderCacheService)
        {
            _reportColumnMappingRepository = reportColumnMappingRepository;
            _columnProviderCacheService = columnProviderCacheService;
        }

        public ReportColumnMapping GetReportColumnMapping(int id)
        {
            //using (var scope = )
            //{
                var result = _reportColumnMappingRepository.GetReportColumnMapping(id);

                return result;
            //}
        }

        public IList<ReportColumnMapping> GetReportColumnMappingsByOrganizationId(int dataSourceTypeId,
            int? organizationId)
        {
            IList<ReportColumnMapping> global = null;
            if (organizationId.HasValue)
            {
                global = GetReportColumnMappingsByOrganizationId(dataSourceTypeId, null);
            }

            // look in cache first
            var fromCache = _columnProviderCacheService.GetMappingsByOrganization(dataSourceTypeId, organizationId);

            if (!fromCache.Any())
            {
                // then get whats missing out of the DB
                //using (var scope = )
                //{
                    var fromDb =
                        _reportColumnMappingRepository.GetReportColumnMappingsByOrganizationId(dataSourceTypeId,
                            organizationId).ToList();
                   

                    if (fromDb.Any())
                    {
                        fromCache.AddRange(fromDb);
                        _columnProviderCacheService.SetMappings(dataSourceTypeId, fromDb);
                    }
                //}
            }

            if (global != null && global.Any())
            {
                fromCache.AddRange(global);
            }

            return fromCache;
        }


        public IList<ReportColumnMapping> GetReportColumnMappingsById(int dataSourceTypeId, IEnumerable<int> columnIds)
        {
            // look in cache first
            var fromCache = _columnProviderCacheService.GetMappingById(dataSourceTypeId, columnIds);

            if (fromCache.Count != columnIds.Count())
            {
                var misssingIds = columnIds.Except(fromCache.Select(x => x.Id)).ToList();

                if (misssingIds.Any())
                {
                    // then get whats missing out of the DB
                    //using (var scope = ))
                    //{
                        var fromDb =
                            _reportColumnMappingRepository.GetReportColumnMappingsById(dataSourceTypeId, misssingIds)
                                .ToList();

                        if (fromDb.Any())
                        {
                            fromCache.AddRange(fromDb);
                            _columnProviderCacheService.SetMappings(dataSourceTypeId, fromDb);
                        }
                    //}
                }
            }

            return fromCache;
        }

        public IList<ReportColumnMapping> Find(int dataSourceTypeId, string table, string field, int? actionSpecId, bool cacheOnly = false)
        {
            // look in cache first
            var fromCache = _columnProviderCacheService.Find(dataSourceTypeId, table, field, actionSpecId);
            
            if (!fromCache.Any())
            {
                // load all into cache and try again
                this.GetReportColumnMappingsByOrganizationId(dataSourceTypeId, null);
                fromCache = _columnProviderCacheService.Find(dataSourceTypeId, table, field, actionSpecId);
            }

            if (!fromCache.Any() && !cacheOnly)
            {
                // then get whats missing out of the DB
                //using (var scope = ))
                //{
                    Debug.WriteLine("Looking for Column in DB {0}.{1}", table, field);

                    var fromDb = _reportColumnMappingRepository.Find(dataSourceTypeId, table, field, actionSpecId).ToList();

                    if (fromDb.Any())
                    {
                        fromCache.AddRange(fromDb);
                        _columnProviderCacheService.SetMappings(dataSourceTypeId, fromDb);
                    }  
                //}
            }
            return fromCache;
        }

        public IList<ReportColumnMapping> Find(int dataSourceTypeId, string uniqueName, bool cacheOnly = true)
        {
            // look in cache first
            var fromCache = _columnProviderCacheService.Find(dataSourceTypeId, uniqueName);

            if (!fromCache.Any() && !cacheOnly)
            {
                // dont cache it incase it affects other items loading
                var fromDb = _reportColumnMappingRepository.Find(dataSourceTypeId, uniqueName);
                return fromDb;
            }
            return fromCache;
        }

        public void ClearCache()
        {
            _columnProviderCacheService.ClearAllMappings();
        }
    }
}
