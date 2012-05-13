namespace FlatBeats.DataModel.Services
{
    using System;
    using System.IO;

    using Microsoft.Phone.Reactive;

    public interface IAsyncStorage
    {
        /// <summary>
        /// TODO: Make async 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool Exists(string filePath);

        IObservable<Unit> SaveStringAsync(string filePath, string text);

        IObservable<Unit> SaveJsonAsync<T>(string filePath, T data) where T : class;

        /// <summary>
        /// TODO: Make async
        /// </summary>
        /// <param name="file"></param>
        /// <param name="data"></param>
        void Save(string filePath, Stream data);

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

        IObservable<T> LoadJsonAsync<T>(string filePath) where T : class;

        IObservable<string> LoadStringAsync(string filePath);
    }
}
