using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using MagiQL.Framework.Model.Response.Base;

namespace MagiQL.Service.WebAPI.Routes.ActionFilters
{
    /// <summary>
    /// A WebAPI ActionFilter that automatically sets the HTTP Status Code of the response
    /// to '500 Server Error' for actions that returned an error (i.e. for actions
    /// that returned our standard ResponseBase API response with its Error propery populated).
    /// </summary>
    public class StatusCodeResponseActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                var objectContent = actionExecutedContext.Response.Content as ObjectContent;
                if (objectContent != null)
                {
                    var response = objectContent.Value as ResponseBase;
                    if (response != null)
                    {
                        SetStatusCode(response, actionExecutedContext);
                    }
                }
            }
            catch
            {
            }
        }

        private void SetStatusCode(ResponseBase response, HttpActionExecutedContext actionExecutedContext)
        {
            if (response.Error != null && response.Error.Message != null)
            {
                actionExecutedContext.Response.StatusCode = HttpStatusCode.InternalServerError;
            }
        }
    }
}