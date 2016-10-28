using System;

namespace MagiQL.Framework.Model.Response.Base
{
    
    public abstract class ResponseBase<T> : ResponseBase
    {
        public T Data { get; set; } 
    }

    public abstract class ResponseBase
    {
        internal Exception OriginalException { get; set; }
        public ResponseError Error { get; set; }
        public ResponseTiming Timing { get; set; }
    }
}