using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using MagiQL.Framework.Model.Response.Base;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System.Linq;

namespace MagiQL.Service.Client
{
    public class ClientBase
    {
        public string BaseUrl = ConfigurationManager.AppSettings["Services.ReportsService.BaseUrl"] ?? "http://magiql-api.local/";
        public static string ApiVersion = "v1";
        public int MaxTries = 1;


        public List<RequestHeader> RequestHeaders { get; set; }

        /// <summary>
        /// Build a RestClient for API requests. 
        /// </summary>
        /// <param name="baseUrl">If set, will use the provided base URL. Otherwise, will use the hardcoded default base URL for LinkedIn API calls.</param>
        protected RestClient BuildRestClient(string baseUrl = null)
        {
            // fix issue with limited response size
            HttpWebRequest.DefaultMaximumErrorResponseLength = 1048576;

            var restClient = new RestClient(baseUrl ?? BaseUrl);
            restClient.UserAgent = "MagiQL/" + Assembly.GetExecutingAssembly().GetName().Version;
            
            //-- Use JSON.NET to get finer control over the deserialization (e.g. to corectly deserialize dates) 
            var deserializer = new CustomJsonDeserializer();
            restClient.AddHandler("application/json", deserializer);
            restClient.AddHandler("text/json", deserializer);
            restClient.AddHandler("text/x-json", deserializer);
            restClient.AddHandler("text/javascript", deserializer);

            //-- HTTP headers and parameters common to all requests 
            restClient.AddDefaultHeader("Content-Type", "application/json");
            restClient.AddDefaultHeader("Accept", "application/json");
            
            return restClient;
        }


        public RestRequest CreateRequest(Method method, string path)
        {
            var apiRequest = new RestRequest(string.Format("/{0}/{1}", ApiVersion, path));
            apiRequest.Method = method;
            apiRequest.RequestFormat = DataFormat.Json;
            apiRequest.JsonSerializer = new RestSharpJsonNetSerializer();

            AddHeaders(apiRequest, RequestHeaders);

            return apiRequest;
        }


        protected void AddHeaders(RestRequest apiRequest, List<RequestHeader> headers)
        {
            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    apiRequest.AddHeader(header.Name, header.Value);
                }
            }
        }


        protected T ExecutePost<T>(RestRequest request, string baseUrl = null) where T : new()
        {
            return Execute<T>(request, baseUrl);
        }

        protected T Execute<T>(RestRequest request, string baseUrl = null) where T : new()
        {
            T result = default(T);

            var executionTimer = Stopwatch.StartNew();
            var createClientTimer = new Stopwatch();


            var nbTries = 0;

            while (nbTries < MaxTries)
            {
                nbTries++;

                createClientTimer.Start();
                var client = BuildRestClient(baseUrl);
                createClientTimer.Stop();

                var response = client.Execute<T>(request);

                if (response.ErrorException != null)throw response.ErrorException;

                if (response.Data == null && response.ErrorMessage != null)
                    throw new Exception(response.ErrorMessage);

                result = response.Data;
                break;
            }

            executionTimer.Stop();
            AddTimingToResponse(result, "ClientExecuteElapsedMilliseconds", executionTimer.ElapsedMilliseconds);
            AddTimingToResponse(result, "ClientCreateElapsedMilliseconds", createClientTimer.ElapsedMilliseconds);

            return result;
        }

        protected async Task<T> ExecuteAsync<T>(RestRequest request, string baseUrl = null) where T : new()
        {
            T result = default(T);

            var executionTimer = Stopwatch.StartNew();
            var createClientTimer = new Stopwatch();


            var nbTries = 0;

            while (nbTries < MaxTries)
            {
                nbTries++;

                createClientTimer.Start();
                var client = BuildRestClient(baseUrl);
                createClientTimer.Stop();

                var response = await client.ExecuteTaskAsync<T>(request);

                if (response.ErrorException != null)
                    throw response.ErrorException;

                result = response.Data;
                break;
            }

            executionTimer.Stop();
            AddTimingToResponse(result, "ClientExecuteElapsedMilliseconds", executionTimer.ElapsedMilliseconds);
            AddTimingToResponse(result, "ClientCreateElapsedMilliseconds", createClientTimer.ElapsedMilliseconds);

            return result;
        }

        private void AddTimingToResponse<T>(T result, string timerName, long valueMilliseconds)
        {
            try
            {
                if (result != null && result is ResponseBase)
                {
                    var response = result as ResponseBase;

                    if (response.Timing == null)
                    {
                        response.Timing = new ResponseTiming();
                    }

                    if (response.Timing.AdditionalTiming == null)
                    {
                        response.Timing.AdditionalTiming = new Dictionary<string, long>();
                    }

                    response.Timing.AdditionalTiming.Add(timerName, valueMilliseconds);
                }
            }
            catch { }
        }

      
    }

    public class CustomJsonDeserializer : IDeserializer
    {
        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
    }

    /// <summary>
    /// A JSON serializer for RestSharp that uses JSON.NET to do the work.
    /// </summary>
    public class RestSharpJsonNetSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer serializer;

        /// <summary>
        /// A JSON serializer for RestSharp that uses JSON.NET to do the work.
        /// 
        /// Knowns how to correctly serialize date/time values.
        /// </summary>
        public RestSharpJsonNetSerializer()
        {
            ContentType = "application/json";
            serializer = new Newtonsoft.Json.JsonSerializer
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,

                // Correctly serialize date time values:
                // 1) DateTime values that are expressed in an unspecified timezone (i.e. DateTime values with a Kind of "Unspecified")
                // must be serialized as a local date (i.e. as a date/time value with no offset from UTC. E.g. "2015-06-15T13:00").
                // This is used in the Reporting service when specifying a date range expressed in "Account Time". In that case, the 
                // timezone that the date is expressed is undefined so we must ensure that these dates are sent as local dates (which
                // is what they are). 
                //
                // 2) DateTime values that represent a specific instant in time (i.e. DateTime values with a Kind of "Utc" or "Local")
                // must be serialized with their UTC offset (so that the receiver is able to know what instant in time that date represents.
                // E.g. "2015-06-15T13:00Z" or "2015-06-15T13:00+02:00").
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            };
        }

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.Formatting = Formatting.Indented;
                    jsonTextWriter.QuoteChar = '"';

                    serializer.Serialize(jsonTextWriter, obj);

                    var result = stringWriter.ToString();
                    return result;
                }
            }
        }

        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string DateFormat { get; set; }
        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string RootElement { get; set; }
        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }
    }
}