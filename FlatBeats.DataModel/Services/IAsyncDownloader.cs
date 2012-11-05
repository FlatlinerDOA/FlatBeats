namespace FlatBeats.DataModel.Services
{
    using System;
    using System.IO;

    using Flatliner.Functional;

    public interface IAsyncDownloader
    {
        bool IsAuthenticated
        {
            get;
        }

        string UserToken { get; set; }

        IObservable<PortableUnit> GetAndSaveFileAsync(Uri url, string fileName, bool overwrite);

        IObservable<T> GetDeserializedCachedAndRefreshedAsync<T>(Uri url, string cacheFile) where T : class;

        IObservable<T> GetDeserializedCachedAsync<T>(Uri url, string cacheFile) where T : class;

        IObservable<Stream> GetStreamAsync(Uri url, bool disableCache);

        IObservable<T> GetDeserializedAsync<T>(Uri url) where T : class;

        IObservable<TResponse> PostAndGetDeserializedAsync<TRequest, TResponse>(Uri url, TRequest postData)
            where TRequest : class where TResponse : class;

        IObservable<string> PostAndGetStringAsync(Uri url, string postData);

        IObservable<TResponse> PostStringAndGetDeserializedAsync<TResponse>(Uri url, string postData)
            where TResponse : class;
    }
}