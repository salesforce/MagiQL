using System.Collections.Generic;

namespace MagiQL.Framework.Model.Response.Base
{
    public class ResponseTiming
    {
        public long ElapsedMilliseconds { get; set; }
        public Dictionary<string, long> AdditionalTiming { get; set; }  
    }
}