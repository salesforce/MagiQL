using System;
using System.IO;
using System.Linq;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Interfaces.Logging;
using MagiQL.Framework.Interfaces.Services;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Request;
using MagiQL.Framework.Model.Response;
using MagiQL.Framework.Model.Response.Base;
using MagiQL.Service.Interfaces;

namespace MagiQL.Framework.Services
{
    public class ReportsService : IReportsService
    {  
        private readonly ISearchService _searchService;
        private readonly ILoggingProvider _logger;
        private readonly IReportsDataSourceFactory _reportsDataSourceFactory;
        private readonly ISearchRequestValidator _searchRequestValidator;
        private readonly IAsyncReportGeneratorService _asyncReportGeneratorService; 

        public ReportsService(
            IReportsDataSourceFactory reportsDataSourceFactory,
            ISearchRequestValidator searchRequestValidator, 
            IAsyncReportGeneratorService asyncReportGeneratorService, 
            ISearchService searchService,
            ILoggingProvider logger)
        {
            _reportsDataSourceFactory = reportsDataSourceFactory;
            _searchRequestValidator = searchRequestValidator;
            _asyncReportGeneratorService = asyncReportGeneratorService;
            _searchService = searchService;
            _logger = logger;
        }


        public SearchResponse Search(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            var result = new SearchResponse { Request = request };
            try
            {
                using (new RequestTimer(result))
                { 
                    _searchRequestValidator.Validate(platform, organizationId, request);
                    var searchResult = _searchService.Search(platform, request);

                    result.Data = searchResult.Data;
                    result.Summary = searchResult.Summary;

                    if (request.DebugMode)
                    {
                        result.DebugInfo = searchResult.DebugInfo;
                    }

                    result.Error = searchResult.Error;
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        public SearchResponse Sql(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            var result = new SearchResponse { Request = request };
            try
            {
                using (new RequestTimer(result))
                {
                    _searchRequestValidator.Validate(platform, organizationId, request);
                    var searchResult = _searchService.Search(platform, request, doNotExecute : true);

                    result.Data = searchResult.Data;
                    result.Summary = searchResult.Summary;
                    result.DebugInfo = searchResult.DebugInfo;
                    result.Error = searchResult.Error;
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }
         

        #region Columns

        public GetSelectableColumnsResponse GetSelectableColumns(string platform, int? organizationId = null, int? userId = null, int? groupBy = null)
        {
            var result = new GetSelectableColumnsResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
                    var columns = dataSource.GetAllSelectableColumnDefinitions(organizationId, groupBy);
                    result.Data = columns;
                } 
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        public GetColumnMappingsResponse GetColumnMappings(string platform, int organizationId, int? userId, int? columnId = null, bool clearCache = false)
        {
            var result = new GetColumnMappingsResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
                    var columnProvider = dataSource.GetColumnProvider();

                    if (clearCache)
                    {
                        columnProvider.ClearCache(dataSource.DataSourceId);
                    }

                    var columns = columnProvider.GetAllColumnMappings(dataSource.DataSourceId, organizationId);
                    if (columnId != null)
                    {
                        columns = columns.Where(x => x.Id == columnId.Value).ToList();
                    }
                    result.Data = columns;
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        public GetColumnMappingsResponse GetDependantColumns(string platform, int columnId)
        {
            var result = new GetColumnMappingsResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
                    var columns = dataSource.GetDependantColumnMappings(dataSource.DataSourceId, columnId); 
                    result.Data = columns;
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }
        public GetColumnMappingsResponse GetDependantColumns(string platform, string fieldName)
        {
            var result = new GetColumnMappingsResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
                    var columns = dataSource.GetDependantColumnMappings(dataSource.DataSourceId, fieldName); 
                    result.Data = columns;
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        public UpdateColumnMappingResponse UpdateColumnMapping(string platform, int columnId, int? userId, ReportColumnMapping columnMapping)
        {
            var result = new UpdateColumnMappingResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
                    var column = dataSource.GetColumnProvider().UpdateColumnMapping(dataSource.DataSourceId, columnId, columnMapping, dataSource.GetColumnValidator());
                    result.Data = column;
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        public CreateColumnMappingResponse CreateColumnMapping(string platform, ReportColumnMapping columnMapping)
        {
            var result = new CreateColumnMappingResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
                    var column = dataSource.GetColumnProvider().CreateColumnMapping(dataSource.DataSourceId, columnMapping, dataSource.GetColumnValidator());
                    result.Data = column;
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        #endregion
        
        public GetTableInfoResponse GetTableInfo(string platform)
        {
            var result = new GetTableInfoResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
                    result.Data = dataSource.GetTableInfo();
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        public GetPlatformsResponse GetPlatforms()
        {
            var result = new GetPlatformsResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    result.Data = _reportsDataSourceFactory.GetPlatformInfo();
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        #region Reports

        public GenerateReportResponse GenerateReport(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            var result = new GenerateReportResponse{Request = request};
            try
            {
                using (new RequestTimer(result))
                {
                    string filePath = Configuration.Exports.FilePath;
                    _searchRequestValidator.Validate(platform, organizationId, request);
                    result.Data = _asyncReportGeneratorService.Setup(platform, organizationId, userId, request, filePath);
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        public GetReportStatusResponse GetReportStatus(string platform, long id, int? userId)
        {
            var result = new GetReportStatusResponse();
            try
            {
                using (new RequestTimer(result))
                {
                    result.Data = _asyncReportGeneratorService.GetStatus(id);
                    if (result.Data.DateCompleted != null)
                    {
                        string fileName = string.Format(Configuration.Exports.FileNameFormat, id);
                        string fullName = Path.Combine(Configuration.Exports.FilePath, fileName);
                        result.FilePath = fullName;
                        result.FileName = fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        public GetConfigurationResponse GetConfiguration(string platform)
        {
            var result = new GetConfigurationResponse {};
            try
            {
                var dataSource = _reportsDataSourceFactory.GetDataSource(platform);
                result.Data = dataSource.GetConfiguration();
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
                _logger.LogException(ex);
            }
            return result;
        }

        #endregion
          
    } 
}
