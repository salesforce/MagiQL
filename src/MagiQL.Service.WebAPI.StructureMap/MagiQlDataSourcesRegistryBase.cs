using System;
using System.Web.Http; 

namespace MagiQL.Service.WebAPI.StructureMap
{ 
    public interface IMagiQlDataSourcesRegistry
    {
        HttpConfiguration GetHttpConfiguration();
        void LogError(Exception ex);
    }
}
