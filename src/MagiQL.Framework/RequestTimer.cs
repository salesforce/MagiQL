using System;
using System.Diagnostics;
using MagiQL.Framework.Model.Response.Base;

namespace MagiQL.Framework
{
    public class RequestTimer : IDisposable
    {
        private readonly ResponseBase _result;
        public Stopwatch Stopwatch;

        public RequestTimer(ResponseBase result)
        {
            _result = result;
            Stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            _result.Timing = new ResponseTiming()
            {
                ElapsedMilliseconds = Stopwatch.ElapsedMilliseconds
            };

        }
    }
}