using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Services
{
    public class ColumnProviderCacheService : IColumnProviderCacheService
    {
        private const int _cacheTimeMinutes = 5;

        public static ConcurrentDictionary<string, CacheItem<ReportColumnMapping>> _cache = new ConcurrentDictionary<string, CacheItem<ReportColumnMapping>>();
        public static List<string> LazyLoadedKeys = new List<string>(); 

        private static object cacheLock = new Object();

        public void ClearAll()
        {
            _cache = new ConcurrentDictionary<string, CacheItem<ReportColumnMapping>>();
        }

        public void ClearMappings(int dataSourceTypeId, int? organizationId)
        {
            lock (cacheLock)
            {
                var matches = _cache.Where(x =>
                    x.Value.Value.DataSourceTypeId == dataSourceTypeId
                    && x.Value.Value.OrganizationId == organizationId).ToList();
                foreach (var m in matches)
                {
                    CacheItem<ReportColumnMapping> val;
                    if (!_cache.TryRemove(m.Key, out val))
                    {
                        
                    }
                }
            }
        }
        public void ClearAllMappings()
        {
            lock (cacheLock)
            {
               _cache.Clear();
            }
        }


        public void SetMappings(int dataSourceTypeId, List<ReportColumnMapping> mappings)
        {
            lock (cacheLock)
            {
                foreach (var column in mappings)
                {
                    var key = BuildKey(dataSourceTypeId, column.OrganizationId, column.Id);

                    if (_cache.ContainsKey(key))
                    {
                        _cache[key] = NewCacheItem(column);
                    }
                    else
                    {
                        if (_cache.TryAdd(key, NewCacheItem(column)))
                        {
                            LazyLoadedKeys.Add(key);
                        }
                        else
                        {
                            
                        }
                    }

                }
            }
        } 


        public List<ReportColumnMapping> GetMappingsByOrganization(int dataSourceTypeId, int? organizationId)
        {
            var matches = _cache.Where(x =>
                 x.Value.Value.DataSourceTypeId == dataSourceTypeId
                && x.Value.Value.OrganizationId == organizationId).ToList();

            var result = RemoveExpired(matches);
            return result;
        }

        public List<ReportColumnMapping> GetMappingById(int dataSourceTypeId, IEnumerable<int> columnIds)
        {
            var matches = _cache.Where(x =>
                 x.Value.Value.DataSourceTypeId == dataSourceTypeId
                && columnIds.Contains(x.Value.Value.Id)).ToList();

            var result = RemoveExpired(matches);
            return result;
        }

        public List<ReportColumnMapping> Find(int dataSourceTypeId, string table, string field, int? actionSpecId)
        {
            var matches = _cache.Where(x => x.Value.Value.DataSourceTypeId == dataSourceTypeId
                                            && x.Value.Value.KnownTable == table
                                            && x.Value.Value.FieldName == field
                                            && x.Value.Value.ActionSpecId == actionSpecId
                                            && x.Value.Value.CreatedByUserId == null
                                            && x.Value.Value.IsCalculated == false
                ).ToList();

            var result = RemoveExpired(matches);
            return result;
        }

        public List<ReportColumnMapping> Find(int dataSourceTypeId, string uniqueName)
        {
            var matches = _cache.Where(x => x.Value.Value.DataSourceTypeId == dataSourceTypeId
                                           && x.Value.Value.UniqueName.ToLower() == uniqueName.ToLower()
               ).ToList();

            var result = RemoveExpired(matches);
            return result;
        }

        public bool IsLazyLoaded(ReportColumnMapping reportColumnMapping)
        {
            var key = BuildKey(reportColumnMapping.DataSourceTypeId, reportColumnMapping.OrganizationId, reportColumnMapping.Id);
            return LazyLoadedKeys.Contains(key);
        }

        private CacheItem<ReportColumnMapping> NewCacheItem(ReportColumnMapping columnMapping)
        {
            var result = new CacheItem<ReportColumnMapping>
            {
                ExpiresTime = GetExpiryTime(columnMapping),
                Value = columnMapping
            };
            return result;
        }

        private DateTime GetExpiryTime(ReportColumnMapping columnMapping)
        {
            var existingForOrg = _cache.Where(x =>
                           x.Value.Value.DataSourceTypeId == columnMapping.DataSourceTypeId
                        && x.Value.Value.OrganizationId == columnMapping.OrganizationId).Select(x => x.Value)
                .FirstOrDefault();

            if (existingForOrg != null)
            {
                return existingForOrg.ExpiresTime;
            }
            return DateTime.Now.AddMinutes(_cacheTimeMinutes);
        }

        private static string BuildKey(int dataSourceTypeId, int? organizationId, int columnId)
        {
            return string.Format("{0}_{1}_{2}",
                dataSourceTypeId,
                organizationId == null ? "global" : organizationId.ToString(),
                columnId
                );
        }

        private static List<ReportColumnMapping> RemoveExpired(List<KeyValuePair<string, CacheItem<ReportColumnMapping>>> matches)
        {
            var result = new List<ReportColumnMapping>();
            foreach (var cache in matches)
            {
                if (cache.Value.ExpiresTime < DateTime.Now)
                {
                    CacheItem<ReportColumnMapping> val;
                    if (_cache.TryRemove(cache.Key, out val))
                    {
                        
                    }
                }
                else
                {
                    result.Add(cache.Value.Value);
                }
            }
            return result;
        }





    }

    public class CacheItem<T>
    {
        public DateTime ExpiresTime { get; set; }
        public T Value { get; set; }
    }
}