namespace FlatBeats.DataModel
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using FlatBeats.DataModel.Services;

    using ServiceStack.Text;

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

    public sealed class StackJson<T> : ISerializer<T> where T : class
    {
        private static readonly JsonSerializer<T> serializer = new JsonSerializer<T>();

        public string SerializeToString(T obj)
        {
            using (new TraceTimer("SerializeToString {0}"))
            {
                return serializer.SerializeToString(obj);
            }
        }

        public T DeserializeFromString(string json)
        {
            using (new TraceTimer("DeserializeFromString {0}"))
            {
                try
                {
                    return serializer.DeserializeFromString(json);
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
        }

        public T DeserializeFromStream(Stream json)
        {
            using (new TraceTimer("DeserializeFromStream {0}"))
            {
                try
                {
                    using (var reader = new StreamReader(json))
                    {
                        return serializer.DeserializeFromReader(reader);
                    }
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
        }

        public void SerializeToStream(T item, Stream stream)
        {
            if (item == null)
            {
                return;
            }

            using (new TraceTimer("SerializeToStream {0}"))
            {
                using (var writer = new StreamWriter(stream))
                {
                    serializer.SerializeToWriter(item, writer);
                }
            }
        }
    }
}