using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Factories
{
    public class ReportsDataSourceFactory : IReportsDataSourceFactory
    {
        private readonly List<IReportsDataSource> _reportsDataSources;

        public ReportsDataSourceFactory(List<IReportsDataSource> reportsDataSources)
        {
            this._reportsDataSources = reportsDataSources;

            var duplicates = _reportsDataSources.GroupBy(x => x.Platform).Where(x => x.Count() > 1);
            if (duplicates.Any())
            {
                throw new Exception(string.Format("The following platforms have been registered more than once : {0}", String.Join(",",duplicates.Select(x=>x.Key))));
            }
        }

        public IReportsDataSource GetDataSource(string platform)
        {
            platform = platform.ToLower().Trim();

            var match = _reportsDataSources.SingleOrDefault(x => x.Platform.ToLower() == platform.ToLower());
            if (match != null)
            {  
                return match;
            }

            throw new Exception(string.Format("No IReportsDataSource available for platform '{0}'", platform));
        }

        public IReportsDataSource GetDataSource(int dataSourceId)
        { 
            var match = _reportsDataSources.SingleOrDefault(x => x.DataSourceId == dataSourceId);
            if (match != null)
            {  
                return match;
            }

            throw new Exception(string.Format("No IReportsDataSource available for dataSourceId '{0}'", dataSourceId));
        }

        public List<PlatformInfo> GetPlatformInfo()
        {
            return _reportsDataSources.Select(ds => new PlatformInfo
            {
                Id = ds.Platform, Name = ds.Platform, DataSourceId = ds.DataSourceId
            }).ToList();
        }
    }
}