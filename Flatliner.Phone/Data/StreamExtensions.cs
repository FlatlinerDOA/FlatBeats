using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Flatliner.Phone.Data
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides simplified manipulation and access to streams.
    /// </summary>
    public static class StreamExtensions
    {
        #region Constants and Fields

        /// <summary>
        /// The default maximum buffer size for all stream operations is 4 kilobytes
        /// </summary>
        public const int DefaultBufferSize = 4096;

        #endregion

        /// <summary>
        /// Closes all streams in a pipeline or enumerable sequence of streams.
        /// </summary>
        /// <param name="streams">The pipeline or enumerable sequence of streams to be closed.</param>
        public static void CloseAll(this IEnumerable<Stream> streams)
        {
            if (streams == null)
            {
                throw new ArgumentNullException("streams");
            }

            foreach (var stream in streams)
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Pumps the data through all the input streams until there is no more data in the first input stream, 
        /// then pipes the entire contents of the last input stream to the output stream.
        /// Uses the default maximum buffer (4096 bytes) at all stages of the process.
        /// </summary>
        /// <param name="inputStreams">The pipeline or enumerable sequence of streams to stream through.</param>
        /// <param name="output">The output stream to write to.</param>
        /// <returns>Returns the output stream, seeked back to the beginning.</returns>
        public static Stream PipeTo(this IEnumerable<Stream> inputStreams, Stream output)
        {
            return inputStreams.PipeTo(output, DefaultBufferSize);
        }

        /// <summary>
        /// Pumps the data through all the input streams until there is no more data in the first input stream, 
        /// then pipes the entire contents of the last input stream to the output stream.
        /// Uses the specified maximum buffer size at all stages of the process.
        /// </summary>
        /// <param name="inputStreams">Enumerable sequence of streams that the data will be pumped through.
        /// The last stream's CanSeek property must return true.</param>
        /// <param name="output">The final output stream</param>
        /// <param name="maxBufferSize">The maximum number of bytes to be transferred at any one time.</param>
        /// <returns>Returns the output stream seeked back to the beginning.</returns>
        public static Stream PipeTo(this IEnumerable<Stream> inputStreams, Stream output, int maxBufferSize)
        {
            if (inputStreams == null)
            {
                throw new ArgumentNullException("inputStreams");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            var finalInput = inputStreams.Last();
            if (!finalInput.CanSeek)
            {
                throw new InvalidCastException(
                    "To pipe to an output stream the last input in the sequence must support to seeking.");
            }

            while (inputStreams.Pump(maxBufferSize))
            {
            }

            finalInput.Seek(0, SeekOrigin.Begin);
            finalInput.PipeTo(output, maxBufferSize);

            if (output.CanSeek)
            {
                output.Seek(0, SeekOrigin.Begin);
            }

            return output;
        }

        /// <summary>
        /// Pipes the entire contents of a stream out to an output stream using the default buffer size (4096 bytes).
        /// </summary>
        /// <param name="input">The input stream to read from (CanRead must return true)</param>
        /// <param name="output">The ouptut stream to write to (CanWrite must return true). </param>
        /// <returns>Returns the output stream seeked back to the beginning.</returns>
        public static Stream PipeTo(this Stream input, Stream output)
        {
            return input.PipeTo(output, DefaultBufferSize);
        }

        /// <summary>
        /// Pipes the entire contents of a stream out to a output stream using the specified buffer size.
        /// Will also attempt to seek the output stream back to the beginning, this allows PipeTo() statements to be chained.
        /// </summary>
        /// <param name="input">The input stream to read from (CanRead must return true)</param>
        /// <param name="output">The ouptut stream to write to (CanWrite must return true). </param>
        /// <param name="maxBufferSize">The maximum number of bytes to transferred at a time.</param>
        /// <returns>Returns the output stream seeked back to the beginning.</returns>
        public static Stream PipeTo(this Stream input, Stream output, int maxBufferSize)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            if (!input.CanRead)
            {
                throw new InvalidOperationException(
                    "Cannot pipe to from an input stream that does not support reading.");
            }

            if (!output.CanWrite)
            {
                throw new InvalidOperationException("Cannot pipe to an output stream that does not support writing.");
            }

            if (input.CanSeek)
            {
                input.Seek(0, SeekOrigin.Begin);
            }

            while (input.PumpTo(output, maxBufferSize) != 0)
            {
            }

            if (output.CanSeek)
            {
                output.Seek(0, SeekOrigin.Begin);
            }

            return output;
        }

        /// <summary>
        /// Pipes the entire contents of a stream out to a binary writer using the default buffer size.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="output">The output binary writer</param>
        public static void PipeTo(this Stream input, BinaryWriter output)
        {
            input.PipeTo(output, DefaultBufferSize);
        }

        /// <summary>
        /// Pipes the entire contents of a stream out to a binary writer using the specified buffer size.
        /// </summary>
        /// <param name="input">The input stream to read from.</param>
        /// <param name="output">The output binary writer</param>
        /// <param name="maxBufferSize">The maximum number of bytes to transferred at a time.</param>
        public static void PipeTo(this Stream input, BinaryWriter output, int maxBufferSize)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            if (!input.CanRead)
            {
                throw new InvalidOperationException(
                    "Cannot pipe to from an input stream that does not support reading.");
            }

            while (input.PumpTo(output, maxBufferSize) != 0)
            {
            }
        }

        /// <summary>
        /// Pumps once through all the streams with the default buffer size (4096 bytes).
        /// </summary>
        /// <param name="inputStreams">The input pipeline or enumerable sequence of streams.</param>
        /// <returns>Returns true if there is more data to be read from the first input stream, otherwise returns false.</returns>
        public static bool Pump(this IEnumerable<Stream> inputStreams)
        {
            return inputStreams.Pump(DefaultBufferSize);
        }

        /// <summary>
        /// Pumps once through all the streams with the specified buffer size.
        /// </summary>
        /// <param name="inputStreams">The input pipeline or enumerable sequence of streams.</param>
        /// <param name="maxBufferSize">The maximum number of bytes to be transferred at once.</param>
        /// <returns>Returns true if there is more data to be read from the first input stream, otherwise returns false.</returns>
        public static bool Pump(this IEnumerable<Stream> inputStreams, int maxBufferSize)
        {
            if (inputStreams == null)
            {
                throw new ArgumentNullException("inputStreams");
            }

            bool hasMore = false;
            var firstInput = inputStreams.FirstOrDefault();
            if (firstInput == null)
            {
                return false;
            }

            var input = firstInput;
            long preWritePosition = 0;
            long postWritePosition = 0;
            foreach (var output in inputStreams.Skip(1))
            {
                if (input == firstInput)
                {
                    // AC 2010-03-25: If the input  is the original source, just pump once.
                    hasMore = input.PumpTo(output, maxBufferSize) != 0;
                }
                else
                {
                    input.Seek(preWritePosition, SeekOrigin.Begin);
                    output.Seek(postWritePosition, SeekOrigin.Begin);
                    preWritePosition = output.Position;

                    // AC 2010-03-25: If the input is downstream, empty it's contents so that we don't get a backlog.
                    while (input.PumpTo(output, maxBufferSize) != 0)
                    {
                    }

                    postWritePosition = output.Position;
                }

                input = output;
            }

            return hasMore;
        }

        /// <summary>
        /// Transfers a single block (or buffer) of data from the input stream to the output stream.
        /// </summary>
        /// <param name="input">The source of the data to be transferred.</param>
        /// <param name="output">The target stream to be written to.</param>
        /// <param name="maxBufferSize">The maximum number of bytes to be transferred.</param>
        /// <returns>Returns the actual number of bytes that were transferred.</returns>
        public static int PumpTo(this Stream input, Stream output, int maxBufferSize)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            byte[] buffer = new byte[maxBufferSize];

            // Read may return anything from 0 to bufferSize.
            int readSize = input.Read(buffer, 0, maxBufferSize);
            output.Write(buffer, 0, readSize);

            // The end of the file is reached.
            return readSize;
        }

        /// <summary>
        /// Transfers a single block (or buffer) of data from the input stream to the output stream.
        /// </summary>
        /// <param name="input">The source of the data to be transferred.</param>
        /// <param name="output">The target binary writer to be written to.</param>
        /// <param name="maxBufferSize">The maximum number of bytes to be transferred.</param>
        /// <returns>Returns the actual number of bytes that were transferred.</returns>
        public static int PumpTo(this Stream input, BinaryWriter output, int maxBufferSize)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            byte[] buffer = new byte[maxBufferSize];

            // Read may return anything from 0 to bufferSize.
            int readSize = input.Read(buffer, 0, maxBufferSize);
            output.Write(buffer, 0, readSize);

            // The end of the file is reached.
            return readSize;
        }

        /// <summary>
        /// Enumerates the contents of a stream, one byte at a time.
        /// Seeks to the beginning of the stream prior to reading if possible.
        /// </summary>
        /// <param name="stream">The stream to read all the data from.</param>
        /// <returns>Returns an enumerable sequence of bytes.</returns>
        public static IEnumerable<byte> ReadAllBytes(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var b = 0;
            while (b != -1)
            {
                b = stream.ReadByte();
                if (b != -1)
                {
                    yield return (byte)b;
                }
            }
        }

        /// <summary>
        /// Enumerates the contents of a stream, in chunked byte blocks. 
        /// The bytes will be read from the beginning of the stream if it supports seeking.
        /// The last buffer's size may vary based on the result from the stream.
        /// </summary>
        /// <param name="stream">The source stream to read from.</param>
        /// <returns>Returns an enumerable sequence of byte arrays with a maximum size of the default buffer size.</returns>
        public static IEnumerable<byte[]> ReadAllBytesBuffered(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return stream.ReadAllBytesBuffered(DefaultBufferSize);
        }

        /// <summary>
        /// Enumerates the contents of a stream, in chunked byte blocks. 
        /// The bytes will be read from the beginning of the stream if it supports seeking.
        /// The last buffer's size may vary based on the result from the stream.
        /// </summary>
        /// <param name="stream">The source stream to read from.</param>
        /// <param name="maxBufferSize">The maximum number of bytes to be read with each iteration.</param>
        /// <returns>Returns an enumerable sequence of byte arrays with a maximum size of the specified number of bytes.</returns>
        public static IEnumerable<byte[]> ReadAllBytesBuffered(this Stream stream, int maxBufferSize)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            int readSize = -1;
            while (readSize != 0)
            {
                var buffer = new byte[maxBufferSize];

                // Read may return anything from 0 to bufferSize.
                readSize = stream.Read(buffer, 0, maxBufferSize);
                if (readSize != 0)
                {
                    if (readSize < buffer.Length)
                    {
                        var output = new byte[readSize];
                        Array.Copy(buffer, 0, output, 0, readSize);
                        yield return output;
                    }
                    else
                    {
                        yield return buffer;
                    }
                }
            }
        }

        /// <summary>
        /// Reads the entire contents of a stream into a string.
        /// Attempts to seek to the beginning of the stream if possible.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>Returns a string of the contents of the stream.</returns>
        public static string ReadAllToString(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// Sets up a new pipeline or enumerable sequence of streams.
        /// Will not tranfer any data until a Pump() or a PipeTo() or is called on the pipeline.
        /// </summary>
        /// <param name="input">The input stream to be read from.</param>
        /// <param name="output">The output stream to be written to.</param>
        /// <returns>Returns a new pipeline or enumerable sequence of streams.</returns>
        public static IEnumerable<Stream> StreamTo(this Stream input, Stream output)
        {
            yield return input;
            yield return output;
        }

        /// <summary>
        /// Adds a stream to an existing pipeline or enumerable sequence of streams. 
        /// Will not transfer any data until Pump() or PipeTo() is called on the pipeline.
        /// </summary>
        /// <param name="inputStreams">The existing pipeline of streams.</param>
        /// <param name="output">The output stream to add tot eh end of the pipeline.</param>
        /// <returns>Returns a new pipeline or enumerable sequence of streams.</returns>
        public static IEnumerable<Stream> StreamTo(this IEnumerable<Stream> inputStreams, Stream output)
        {
            var sourceStream = inputStreams.FirstOrDefault();
            if (sourceStream == null)
            {
                yield return output;
            }
            else if (!sourceStream.CanRead)
            {
                throw new InvalidOperationException("Origin stream does not support being read from");
            }

            yield return sourceStream;

            foreach (var stream in inputStreams.Skip(1))
            {
                if (!stream.CanWrite)
                {
                    throw new InvalidOperationException("One of the target streams does not support writing to.");
                }

                if (!stream.CanSeek)
                {
                    throw new InvalidOperationException(
                        "One of the target streams does not support seeking which is required for the Pump operation.");
                }

                yield return stream;
            }

            yield return output;
        }

        /// <summary>
        /// Pipes the entire contents of a stream to a new memory stream and returns the result.
        /// </summary>
        /// <param name="stream">The stream to pipe the entire contents to the new memory stream.</param>
        /// <returns>Returns a new <see cref="MemoryStream"/> with the contents of the original stream.</returns>
        public static MemoryStream ToMemoryStream(this Stream stream)
        {
            var outputStream = new MemoryStream();
            stream.PipeTo(outputStream);
            return outputStream;
        }

    }
}
