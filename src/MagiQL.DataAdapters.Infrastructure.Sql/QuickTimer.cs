using System;
using System.Diagnostics;

namespace MagiQL.DataAdapters.Infrastructure.Sql
{
    public class QuickTimer : IDisposable
    {
        private readonly Func<long, long> saveValue;
        private readonly Stopwatch stopwatch;

        public QuickTimer(Func<long, long> saveValue)
        {
            this.saveValue = saveValue;
            this.stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            saveValue.Invoke(stopwatch.ElapsedMilliseconds);
        }
    }
}