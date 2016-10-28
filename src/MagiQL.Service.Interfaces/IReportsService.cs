using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;

namespace MagiQL.Service.Interfaces
{
    public interface IReportsService
    {

        SearchResponse Search(string platform, int? organizationId, int? userId, SearchRequest request);

        SearchResponse Sql(string platform, int? organizationId, int? userId, SearchRequest request);
         

        #region Columns

        GetSelectableColumnsResponse GetSelectableColumns(string platform, int? organizationId = null, int? userId = null, int? groupBy = null);

        GetColumnMappingsResponse GetColumnMappings(string platform, int organizationId, int? userId, int? columnId = null, bool clearCache = false);

        GetColumnMappingsResponse GetDependantColumns(string platform, int columnId);

        GetColumnMappingsResponse GetDependantColumns(string platform, string fieldName);

        UpdateColumnMappingResponse UpdateColumnMapping(string platform, int columnId, int? userId, ReportColumnMapping columnMapping);

        CreateColumnMappingResponse CreateColumnMapping(string platform, ReportColumnMapping columnMapping);

        #endregion

        GetPlatformsResponse GetPlatforms();
   
        GetTableInfoResponse GetTableInfo(string platform);

        #region Reports

        GenerateReportResponse GenerateReport(string platform, int? organizationId, int? userId, SearchRequest request);

        GetReportStatusResponse GetReportStatus(string platform, long id, int? userId);

        #endregion

        GetConfigurationResponse GetConfiguration(string platform);
    }
}
