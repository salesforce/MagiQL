using System;
using System.Threading.Tasks;
using MagiQL.Framework.Model.Columns;
using MagiQL.Framework.Model.Response;
using MagiQL.Framework.Model.Response.Base;
using MagiQL.Service.Interfaces;
using MagiQL.Framework.Model.Request;
using RestSharp;

namespace MagiQL.Service.Client
{
    public class ReportsServiceClient : ClientBase, IReportsService
    { 
        public SearchResponse Search(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            var result = new SearchResponse { Request = request };
            
            var apiRequest = SetupSearchRequest(platform, organizationId, userId, request);
            
            try
            {
                // POST  
                var response = Execute<SearchResponse>(apiRequest);
                return response;
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        public SearchResponse Sql(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            var result = new SearchResponse { Request = request };

            var apiRequest = SetupSqlRequest(platform, organizationId, userId, request);

            try
            {
                // POST  
                var response = Execute<SearchResponse>(apiRequest);
                return response;
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }


        public async Task<SearchResponse> SearchAsync(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            var apiRequest = SetupSearchRequest(platform, organizationId, userId, request);
            
            try
            {
                // POST  
                var response = await ExecuteAsync<SearchResponse>(apiRequest);
                return response;
            }
            catch (Exception ex)
            {
                var result = new SearchResponse { Request = request };
                result.Error = new ResponseError().Load(ex);
                return result;
            }
        }
        
        // seperate method because this is used by syn and async versions
        private RestRequest SetupSearchRequest(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            if (platform == null) { throw new ArgumentException("platform"); }
            if (organizationId == null) { throw new ArgumentException("organizationId"); }
          
            var apiRequest = CreateRequest(Method.POST, "{platform}/search");
            apiRequest.AddUrlSegment("platform", platform);
            apiRequest.AddParameter("organizationid", organizationId, ParameterType.QueryString);
            if (userId != null)
            {
                apiRequest.AddParameter("userid", userId, ParameterType.QueryString);
            }
            apiRequest.AddBody(request);
            return apiRequest;
        }


        // seperate method because this is used by syn and async versions
        private RestRequest SetupSqlRequest(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            if (platform == null) { throw new ArgumentException("platform"); }
            if (organizationId == null) { throw new ArgumentException("organizationId"); }

            var apiRequest = CreateRequest(Method.POST, "{platform}/sql"); 
            apiRequest.AddUrlSegment("platform", platform);
            apiRequest.AddParameter("organizationid", organizationId, ParameterType.QueryString);
            if (userId != null)
            {
                apiRequest.AddParameter("userid", userId, ParameterType.QueryString);
            }
            apiRequest.AddBody(request);
            return apiRequest;
        }
        
        #region Columns

        public GetSelectableColumnsResponse GetSelectableColumns(string platform, int? organizationId = null, int? userId = null, int? groupBy = null)
        {
            var apiRequest = SetupGetSelectableColumnsRequest(platform, organizationId, userId, groupBy);

            var result = new GetSelectableColumnsResponse();
            try
            {
                // GET   
                var response = Execute<GetSelectableColumnsResponse>(apiRequest);
                return response;

            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

      
        public async Task<GetSelectableColumnsResponse> GetSelectableColumnsAsync(string platform, int? organizationId = null, int? userId = null, int? groupBy = null)
        {
            var result = new GetSelectableColumnsResponse();
            var apiRequest = SetupGetSelectableColumnsRequest(platform, organizationId, userId, groupBy);

            try
            {
                // GET  
                var response = await ExecuteAsync<GetSelectableColumnsResponse>(apiRequest);
                return response;

            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }
         
        public GetColumnMappingsResponse GetColumnMappings(string platform, int organizationId, int? userId, int? columnId = null, bool clearCache = false)
        {
            var result = new GetColumnMappingsResponse();
            try
            {
                // GET 
                var apiRequest = CreateRequest(Method.GET, "{platform}/columnMappings");
                apiRequest.AddUrlSegment("platform", platform);
                apiRequest.AddParameter("organizationid", organizationId, ParameterType.QueryString);
                if (userId != null)
                {
                    apiRequest.AddParameter("userid", userId, ParameterType.QueryString);
                }
                if (columnId != null)
                {
                    apiRequest.AddParameter("columnid", columnId, ParameterType.QueryString);    
                }
                if (clearCache)
                {
                    apiRequest.AddParameter("clearcache", true, ParameterType.QueryString);    
                }
                var response = Execute<GetColumnMappingsResponse>(apiRequest);
                return response;
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        public GetColumnMappingsResponse GetDependantColumns(string platform, int columnId)
        {
            var result = new GetColumnMappingsResponse();
            try
            {
                // GET 
                var apiRequest = CreateRequest(Method.GET, "{platform}/dependantColumnMappings");
                apiRequest.AddUrlSegment("platform", platform);  
                apiRequest.AddParameter("columnid", columnId, ParameterType.QueryString);
                var response = Execute<GetColumnMappingsResponse>(apiRequest);
                return response;
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        public GetColumnMappingsResponse GetDependantColumns(string platform, string fieldName)
        {
            var result = new GetColumnMappingsResponse();
            try
            {
                // GET 
                var apiRequest = CreateRequest(Method.GET, "{platform}/dependantColumnMappings");
                apiRequest.AddUrlSegment("platform", platform);
                apiRequest.AddParameter("fieldname", fieldName, ParameterType.QueryString);
                var response = Execute<GetColumnMappingsResponse>(apiRequest);
                return response;
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        // seperate method because this is used by syn and async versions
        private RestRequest SetupGetSelectableColumnsRequest(string platform, int? organizationId, int? userId, int? groupBy)
        {
            var apiRequest = CreateRequest(Method.GET, "{platform}/columns");
            apiRequest.AddUrlSegment("platform", platform);
            if (organizationId != null)
            {
                apiRequest.AddParameter("organizationid", organizationId, ParameterType.QueryString);
            }
            if (userId != null)
            {
                apiRequest.AddParameter("userid", userId, ParameterType.QueryString);
            }
            if (groupBy != null)
            {
                apiRequest.AddParameter("groupby", groupBy, ParameterType.QueryString);
            }
            return apiRequest;
        }



        public UpdateColumnMappingResponse UpdateColumnMapping(string platform, int columnId, int? userId, ReportColumnMapping columnMapping)
        {
            var result = new UpdateColumnMappingResponse();
            try
            {
                // PUT 
                var apiRequest = CreateRequest(Method.PUT, "{platform}/columnMappings/{columnId}");
                apiRequest.AddUrlSegment("platform", platform);
                apiRequest.AddUrlSegment("columnId", columnId.ToString());
                if (userId != null)
                {
                    apiRequest.AddParameter("userid", userId, ParameterType.QueryString);
                }
                apiRequest.AddBody(columnMapping);
                var response = Execute<UpdateColumnMappingResponse>(apiRequest);
                return response;

            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        public CreateColumnMappingResponse CreateColumnMapping(string platform, ReportColumnMapping columnMapping)
        {
            var result = new CreateColumnMappingResponse();
            try
            {
                // POST  
                var apiRequest = CreateRequest(Method.POST, "{platform}/columnMappings");
                apiRequest.AddUrlSegment("platform", platform); 
                apiRequest.AddBody(columnMapping);;
                var response = Execute<CreateColumnMappingResponse>(apiRequest);
                return response;

            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }


        #endregion


        public GetTableInfoResponse GetTableInfo(string platform)
        {
            var result = new GetTableInfoResponse();
            try
            {
                // POST  
                var apiRequest = CreateRequest(Method.GET, "{platform}/tableInfo");
                apiRequest.AddUrlSegment("platform", platform);
                var response = Execute<GetTableInfoResponse>(apiRequest);
                return response;

            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        public GetPlatformsResponse GetPlatforms()
        {
            var result = new GetPlatformsResponse();
            try
            {
                // POST  
                var apiRequest = CreateRequest(Method.GET, "platforms");
                var response = Execute<GetPlatformsResponse>(apiRequest);
                return response;

            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        public GetConfigurationResponse GetConfiguration(string platform)
        {
            var result = new GetConfigurationResponse();
            try
            {
                // POST  
                var apiRequest = CreateRequest(Method.GET, "{platform}/configuration");
                apiRequest.AddUrlSegment("platform", platform);
                var response = Execute<GetConfigurationResponse>(apiRequest);
                return response;

            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        #region Reports
        
        public GenerateReportResponse GenerateReport(string platform, int? organizationId, int? userId, SearchRequest request)
        {
            var result = new GenerateReportResponse {Request = request};
            try
            { 
                // POST  
                var apiRequest = CreateRequest(Method.POST, "{platform}/reports");
                apiRequest.AddUrlSegment("platform", platform);
                apiRequest.AddParameter("organizationid", organizationId, ParameterType.QueryString); 
                if (userId != null)
                {
                    apiRequest.AddParameter("userid", userId, ParameterType.QueryString);
                }
                apiRequest.AddBody(request);

                var response = Execute<GenerateReportResponse>(apiRequest);
                return response;
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }

        public GetReportStatusResponse GetReportStatus(string platform, long id, int? userId = null)
        {
            var result = new GetReportStatusResponse();
            try
            { 
                // GET  
                var apiRequest = CreateRequest(Method.GET, "{platform}/reports/{id}");
                apiRequest.AddUrlSegment("platform", platform);
                apiRequest.AddParameter("id", id, ParameterType.QueryString);
                if (userId != null)
                {
                    apiRequest.AddParameter("userid", userId, ParameterType.QueryString);
                }

                var response = Execute<GetReportStatusResponse>(apiRequest);
                return response;
            }
            catch (Exception ex)
            {
                result.Error = new ResponseError().Load(ex);
            }
            return result;
        }


        #endregion
 
    }
}
