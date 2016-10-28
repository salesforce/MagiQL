using System;
using System.Diagnostics;

namespace MagiQL.DataAdapters.Infrastructure.Sql
{
    // used to debug slow performing code, please dont remove this, it can be disabled using _enabled = false

    public class DebugTimer : IDisposable
    {
        private readonly string _message;
        private Stopwatch _stopwatch;
        private DateTime _startTime;

        private bool _enabled = false;

        public DebugTimer(string message)
        {
            if (_enabled)
            {
                _startTime = DateTime.Now;
                _message = message;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }
        }

        public void Dispose()
        {
            if (_enabled)
            {
                _stopwatch.Stop();
                Debug.WriteLine("{0} :: {1} {2} took {3} ms ({4})",
                    _startTime.ToString("hh:mm:ss.fff"),
                    DateTime.Now.ToString("hh:mm:ss.fff"),
                    _message, //0, 0);
                    _stopwatch.ElapsedMilliseconds,
                    _stopwatch.ElapsedTicks);
                _stopwatch = null;
            }
        }
    }
}
