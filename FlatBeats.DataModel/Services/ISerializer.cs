namespace FlatBeats.DataModel.Services
{
    using System.IO;

    public interface ISerializer<T> where T : class
    {
        string SerializeToString(T obj);

        void SerializeToStream(T item, Stream stream);

        T DeserializeFromString(string json);

        /// <summary>
        /// Closes the stream after deserializing??
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        T DeserializeFromStream(Stream json);
    }
}