using System;
using Newtonsoft.Json;

namespace MagiQL.Framework.Model.Response.Base
{
    public class ResponseError
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }

        [JsonIgnore]
        public Exception Exception { get; set; }


        public ResponseError Load(Exception ex)
        {
            Exception = ex; 
            Message = InnermostException(ex).Message;
            StackTrace = ex.StackTrace;
            return this;
        }

        private Exception InnermostException(Exception e)
        {
            while (e.InnerException != null) e = e.InnerException;
            return e;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}