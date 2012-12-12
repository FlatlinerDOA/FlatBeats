//--------------------------------------------------------------------------------------------------
// <copyright file="Downloader.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;
    using Flatliner.Functional;

    /// <summary>
    /// </summary>
    public sealed class AsyncDownloader : IAsyncDownloader
    {
        public static readonly IAsyncDownloader Instance = new AsyncDownloader();

        #region Constants and Fields

        private readonly IAsyncStorage storage;

        private readonly object syncRoot = new object();

        /// <summary>
        /// </summary>
        private string userToken;

        #endregion

        public AsyncDownloader() : this(AsyncIsolatedStorage.Instance)
        {
        }

        public AsyncDownloader(IAsyncStorage storage)
        {
            this.storage = storage;
        }

        #region Public Properties

        /// <summary>
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return this.UserToken != null;
            }
        }

        public string UserToken
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.userToken;
                }
            }

            set
            {
                lock (this.syncRoot)
                {
                    this.userToken = value;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name = "url">
        /// </param>
        /// <param name = "fileName">
        /// </param>
        /// <param name="overwrite">A value indicating whether to overwrite any existing file</param>
        /// <returns>
        /// </returns>
        public IObservable<PortableUnit> GetAndSaveFileAsync(Uri url, string fileName, bool overwrite)
        {
            if (!overwrite && this.storage.Exists(fileName))
            {
                return ObservableEx.SingleUnit();
            }

            return this.GetStreamAsync(url, false).TrySelect(
                        stream =>
                        {
                            using (stream)
                            {
                                this.storage.Save(fileName, stream);
                            }

                            return ObservableEx.Unit;
                        });
        }

        public IObservable<T> GetDeserializedCachedAndRefreshedAsync<T>(Uri url, string cacheFile) where T : class
        {
            IObservable<T> sequence = Observable.Empty<T>();
            if (this.storage.Exists(cacheFile))
            {
                sequence = this.storage.LoadJsonAsync<T>(cacheFile);
            }

            return sequence.Concat(
                from cache in this.GetStreamAsync(url, false).Select(Json<T>.Instance.DeserializeFromStream)
                from _ in this.storage.SaveJsonAsync(cacheFile, cache)
                select cache);
        }


        public IObservable<T> GetDeserializedCachedAsync<T>(Uri url, string cacheFile) where T : class
        {
            if (this.storage.Exists(cacheFile))
            {
                return this.storage.LoadJsonAsync<T>(cacheFile);
            }

            return from cache in this.GetStreamAsync(url, false).Select(Json<T>.Instance.DeserializeFromStream)
                   from _ in this.storage.SaveJsonAsync<T>(cacheFile, cache)
                   select cache;
        }

        public IObservable<Stream> GetStreamAsync(Uri url, bool disableCache)
        {
            return this.WebRequestAsync(url, disableCache).TrySelect(
                r =>
                {
                    Stream c = new MemoryStream();
                    using (var s = r.GetResponseStream())
                    {
                        s.CopyTo(c);
                    }

                    c.Position = 0;
                    return c;
                });
        }

        /// <summary>
        /// Gets Json forcing no-cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public IObservable<T> GetDeserializedAsync<T>(Uri url) where T : class
        {
            return this.GetStreamAsync(url, true).Select(Json<T>.Instance.DeserializeFromStream);
        }

        /// <summary>
        /// </summary>
        /// <param name = "url">
        /// </param>
        /// <param name = "postData">
        /// </param>
        /// <typeparam name = "TRequest">
        /// </typeparam>
        /// <typeparam name = "TResponse">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public IObservable<TResponse> PostAndGetDeserializedAsync<TRequest, TResponse>(Uri url, TRequest postData)
            where TRequest : class where TResponse : class
        {
            var sequence = from postString in ObservableEx.DeferredStart(() => Json<TRequest>.Instance.SerializeToString(postData))
                           from completed in this.PostAndGetStringAsync(url, postString)
                           select Json<TResponse>.Instance.DeserializeFromString(completed);
            return sequence;
        }

        /// <summary>
        /// </summary>
        /// <param name = "url">
        /// </param>
        /// <param name = "postData">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<string> PostAndGetStringAsync(Uri url, string postData)
        {
            return Observable.Using(
                () =>
                {
                    var r = this.CreateRequest(url, true);
                    r.Method = "POST";
                    r.ContentType = "application/x-www-form-urlencoded";
                    return r;
                },
                r =>
                {
                    Debug.WriteLine("GET " + url.OriginalString);

                    return from requestStream in Observable.FromAsyncPattern<Stream>(r.BeginGetRequestStream, r.EndGetRequestStream)().Do(
                               s =>
                               {
                                   var data = Encoding.UTF8.GetBytes(postData);
                                   s.Write(data, 0, data.Length);
                                   s.Flush();
                                   s.Close();
                               })
                           from response in Observable.FromAsyncPattern<WebResponse>((c, st) => r.BeginGetResponse(c, st), (ar) => r.EndGetResponse(ar))()
                           .Catch<WebResponse, WebException>(DownloadExtensions.HandleWebException<WebResponse>)
                           select response;
                }).TrySelect(t =>
                {
                    using (var responseStream = t.GetResponseStream())
                    {
                        var c = new MemoryStream();
                        responseStream.CopyTo(c);
                        c.Position = 0;
                        using (var sr = new StreamReader(c))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                });
        }


        /// <summary>
        /// </summary>
        /// <param name = "url">
        /// </param>
        /// <param name = "postData">
        /// </param>
        /// <typeparam name = "TResponse">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public IObservable<TResponse> PostStringAndGetDeserializedAsync<TResponse>(Uri url, string postData)
            where TResponse : class
        {
            var sequence = from completed in this.PostAndGetStringAsync(url, postData)
                           select Json<TResponse>.Instance.DeserializeFromString(completed);
            return sequence;
        }

        #endregion

        #region Methods

        private IObservable<WebResponse> WebRequestAsync(Uri address, bool noCache)
        {
            return Observable.Using(
                () => this.CreateRequest(address, noCache),
                r =>
                {
                    Debug.WriteLine("GET " + address.OriginalString);
                    return Observable.FromAsyncPattern<WebResponse>(r.BeginGetResponse, r.EndGetResponse)();
                }).Catch<WebResponse, WebException>(DownloadExtensions.HandleWebException<WebResponse>);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private DisposableWebRequest CreateRequest(Uri address, bool noCache)
        {
            var request = new DisposableWebRequest(address);
            request.Headers["X-Api-Key"] = "9abd1c4181d59dbece062455b941e64da474e5c7";

            if (this.IsAuthenticated)
            {
                request.Headers["X-User-Token"] = this.UserToken;
            }

            if (noCache)
            {
                request.Headers[HttpRequestHeader.Pragma] = "no-cache";
            }

            return request;
        }

        #endregion
    }
}