using System.Collections.Generic;
using MagiQL.Framework.Model.Columns;

namespace MagiQL.Framework.Interfaces
{
    public interface IReportsDataSourceFactory
    {
        IReportsDataSource GetDataSource(string platform);
        IReportsDataSource GetDataSource(int dataSourceId);
        List<PlatformInfo> GetPlatformInfo();
    }
}