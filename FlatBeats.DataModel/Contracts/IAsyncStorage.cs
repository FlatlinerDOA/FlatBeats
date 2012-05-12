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

namespace FlatBeats.DataModel.Contracts
{
    using System.IO;

    using Microsoft.Phone.Reactive;

    public interface IAsyncStorage
    {
        /// <summary>
        /// TODO: Make async 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool Exists(string file);

        IObservable<Unit> SaveStringAsync(string file, string text);

        IObservable<Unit> SaveJsonAsync<T>(string file, T data) where T : class;

        /// <summary>
        /// TODO: Make async
        /// </summary>
        /// <param name="file"></param>
        /// <param name="data"></param>
        void Save(string file, Stream data);

        /// <summary>
        /// TODO: Make async
        /// </summary>
        /// <param name="file"></param>
        void Delete(string file);

        /// <summary>
        /// TODO: Make async
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Stream ReadStream(string filePath);

        IObservable<T> LoadJsonAsync<T>(string file) where T : class;

        IObservable<string> LoadStringAsync(string file);
    }

    public interface IAsyncDownloader
    {
        
    }

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
