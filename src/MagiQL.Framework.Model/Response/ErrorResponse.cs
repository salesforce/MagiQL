using System;
using MagiQL.Framework.Model.Response.Base;

namespace MagiQL.Framework.Model.Response
{
    /// <summary>
    /// Our standard error response sent when an unhandled exception occurred.
    /// </summary>
    public class ErrorResponse : ResponseBase
    {
        public ErrorResponse(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException("ex");

            Error = new ResponseError().Load(ex);

            // TODO: see if we could also include the request details here somehow to be more helpful
        }
    }
}
