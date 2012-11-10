namespace Flatliner.Phone.Core
{
    using System;
    using System.Diagnostics;

    public struct TraceTimer : IDisposable
    {
        private readonly Stopwatch stopwatch;

        private readonly string messageFormat;

        public TraceTimer(string messageFormat)
        {
            this.messageFormat = messageFormat;
            this.stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            this.stopwatch.Stop();
            Debug.WriteLine(messageFormat, stopwatch.Elapsed);
        }
    }
}
