//--------------------------------------------------------------------------------------------------
// <copyright file="Downloader.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.DataModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;

    using FlatBeats.DataModel.Services;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;

    using System.Text;

    /// <summary>
    /// </summary>
    public class Downloader : IAsyncDownloader
    {
        public static readonly Downloader Instance = new Downloader();

        #region Constants and Fields

        private readonly AsyncIsolatedStorage storage = AsyncIsolatedStorage.Instance;

        private readonly object SyncRoot = new object();

        private UserCredentialsContract userCredentials;

        /// <summary>
        /// </summary>
        private string userToken;

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return UserToken != null;
            }
        }

        ////public UserCredentialsContract UserCredentials
        ////{
        ////    get
        ////    {
        ////        lock (SyncRoot)
        ////        {
        ////            return userCredentials;
        ////        }
        ////    }

        ////    set
        ////    {
        ////        lock (SyncRoot)
        ////        {
        ////            userCredentials = value;
        ////        }
        ////    }
        ////}

        public string UserToken
        {
            get
            {
                lock (SyncRoot)
                {
                    return userToken;
                }
            }

            set
            {
                lock (SyncRoot)
                {
                    userToken = value;
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
        public IObservable<Unit> GetAndSaveFileAsync(Uri url, string fileName, bool overwrite)
        {
            if (!overwrite && storage.Exists(fileName))
            {
                return ObservableEx.SingleUnit();
            }

            return GetStreamAsync(url, false).TrySelect(
                        stream =>
                        {
                            storage.Save(fileName, stream);
                            return ObservableEx.Unit;
                        });
        }

        public IObservable<T> GetDeserializedCachedAndRefreshedAsync<T>(Uri url, string cacheFile) where T : class
        {
            IObservable<T> sequence = Observable.Empty<T>();
            if (storage.Exists(cacheFile))
            {
                sequence = storage.LoadJsonAsync<T>(cacheFile);
            }

            return sequence.Concat(
                from cache in GetStreamAsync(url, false).Select(Json<T>.Instance.DeserializeFromStream)
                from _ in storage.SaveJsonAsync(cacheFile, cache)
                select cache);
        }


        public IObservable<T> GetDeserializedCachedAsync<T>(Uri url, string cacheFile) where T : class
        {
            if (storage.Exists(cacheFile))
            {
                return storage.LoadJsonAsync<T>(cacheFile);
            }

            return from cache in GetStreamAsync(url, false).Select(Json<T>.Instance.DeserializeFromStream)
                   from _ in storage.SaveJsonAsync<T>(cacheFile, cache)
                   select cache;
        }

        public IObservable<Stream> GetStreamAsync(Uri url, bool disableCache)
        {
            return WebRequestAsync(url, disableCache).TrySelect(
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
            return GetStreamAsync(url, true).Select(Json<T>.Instance.DeserializeFromStream);
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
                           from completed in PostAndGetStringAsync(url, postString)
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
                    var r = CreateRequest(url, true);
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
            var sequence = from completed in PostAndGetStringAsync(url, postData)
                           select Json<TResponse>.Instance.DeserializeFromString(completed);
            return sequence;
        }

        #endregion

        #region Methods

        private IObservable<WebResponse> WebRequestAsync(Uri address, bool noCache)
        {
            return Observable.Using(
                () => CreateRequest(address, noCache),
                r =>
                {
                    Debug.WriteLine("GET " + address.OriginalString);
                    return Observable.FromAsyncPattern<WebResponse>(r.BeginGetResponse, r.EndGetResponse)();
                });
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private DisposableWebRequest CreateRequest(Uri address, bool noCache)
        {
            var request = new DisposableWebRequest(address);
            request.Headers["X-Api-Key"] = "9abd1c4181d59dbece062455b941e64da474e5c7";

            if (IsAuthenticated)
            {
                request.Headers["X-User-Token"] = UserToken;
            }

            if (noCache)
            {
                request.Headers[HttpRequestHeader.Pragma] = "no-cache";
            }

            return request;
        }

        #endregion
    }

    internal static class DownloadExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name = "items">
        /// </param>
        /// <param name = "selector">
        /// </param>
        /// <typeparam name = "T">
        /// </typeparam>
        /// <typeparam name = "TResult">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public static IObservable<TResult> TrySelect<T, TResult>(this IObservable<T> items, Func<T, TResult> selector)
        {
            return Observable.CreateWithDisposable<TResult>(
                d => items.Subscribe(
                    item =>
                    {
                        try
                        {
                            TResult result = selector(item);
                            d.OnNext(result);
                        }
                        catch (WebException webException)
                        {
                            Exception newError = ConvertWebException(webException);
                            d.OnError(newError);

                        }
                        catch (Exception ex)
                        {
                            d.OnError(ex);
                        }
                    },
                    d.OnError,
                    d.OnCompleted));
        }

        public static IObservable<T> HandleWebException<T>(WebException ex)
        {
            return Observable.Throw<T>(ConvertWebException(ex));
        }

        private static Exception ConvertWebException(WebException webException)
        {
            Exception newError = webException;
            if (webException.Response != null)
            {
                using (var s = webException.Response.GetResponseStream())
                {
                    var b = new MemoryStream();
                    s.CopyTo(b);
                    b.Position = 0;
                    using (var sr = new StreamReader(b))
                    {
                        var response = Json<ResponseContract>.Instance.DeserializeFromString(sr.ReadToEnd());
                        if (response != null)
                        {
                            newError = new ServiceException(response.Errors, webException, response.ResponseStatus);
                        }
                    }
                }
            }

            return newError;
        }   
    }
}