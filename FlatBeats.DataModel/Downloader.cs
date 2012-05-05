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
    using System.Runtime.Serialization;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;

    using SharpGIS;
    using System.Text;

    /// <summary>
    /// </summary>
    public static class Downloader
    {
        #region Constants and Fields

        private static readonly object SyncRoot = new object();

        private static UserCredentialsContract userCredentials;

        /// <summary>
        /// </summary>
        private static string userToken;

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public static bool IsAuthenticated
        {
            get
            {
                return UserToken != null || UserCredentials != null;
            }
        }

        public static UserCredentialsContract UserCredentials
        {
            get
            {
                lock (SyncRoot)
                {
                    return userCredentials;
                }
            }

            set
            {
                lock (SyncRoot)
                {
                    userCredentials = value;
                }
            }
        }

        public static string UserToken
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
        /// <returns>
        /// </returns>
        public static IObservable<Unit> GetAndSaveFile(Uri url, string fileName)
        {
            return GetStream(url, false).TrySelect(
                        stream =>
                        {
                            Storage.Save(fileName, stream);
                            return ObservableEx.Unit;
                        });
                                             

            /*
            var webRequest = from client in Observable.Return(CreateClient(false))
                             from completed in Observable.CreateWithDisposable<OpenReadCompletedEventArgs>(
                                 observer =>
                                     {
                                         var subscription = Observable.FromEvent<OpenReadCompletedEventArgs>(client, "OpenReadCompleted").Take(1).Select(e => e.EventArgs).Subscribe(observer);
#if DEBUG
                                         Debug.WriteLine("GET " + url.AbsoluteUri);
#endif
                                         client.OpenReadAsync(url);
                                         return subscription;
                                     }).TrySelect(
                                         evt =>
                                             {
                                                 if (evt.Error != null)
                                                 {
                                                     Debug.WriteLine("DOWNLOAD: " + evt.Error.ToString());
                                                 }

                                                 Storage.Save(fileName, evt.Result);
                                                 return ObservableEx.Unit;
                                             })
                             select completed;

            return webRequest;*/
        }

        public static IObservable<T> GetJsonCachedAndRefreshed<T>(Uri url, string cacheFile) where T : class
        {
            IObservable<T> sequence = Observable.Empty<T>();
            if (Storage.Exists(cacheFile))
            {
                sequence = Storage.LoadJsonAsync<T>(cacheFile);
            }

            return sequence.Concat(
                from cache in GetStream(url, false).Select(Json<T>.DeserializeAndClose)
                from _ in Storage.SaveJsonAsync(cacheFile, cache)
                select cache);
        }


        public static IObservable<T> GetJsonCached<T>(Uri url, string cacheFile) where T : class
        {
            if (Storage.Exists(cacheFile))
            {
                return Storage.LoadJsonAsync<T>(cacheFile);
            }

            return from cache in GetStream(url, false).Select(Json<T>.DeserializeAndClose)
                   from _ in Storage.SaveJsonAsync<T>(cacheFile, cache)
                   select cache;
        }

        public static IObservable<Stream> GetStream(Uri url, bool disableCache)
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
            /*
            return from client in Observable.Return(CreateClient(disableCache)).SubscribeOn(Scheduler.ThreadPool)
                   from completed in Observable.CreateWithDisposable<OpenReadCompletedEventArgs>(
                       observer =>
                           {
                               var subscription =
                                   Observable.FromEvent<OpenReadCompletedEventArgs>(client, "OpenReadCompleted").Take(1)
                                       .Select(e => e.EventArgs).Subscribe(observer);
#if DEBUG
                               Debug.WriteLine("GET " + url.AbsoluteUri);
#endif
                               client.OpenReadAsync(url);
                               return subscription;
                           }).TrySelect(evt => evt.Result)
                       select completed;
          */
        }

        /// <summary>
        /// Gets Json forcing no-cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<T> GetJson<T>(Uri url) where T : class
        {
            return GetStream(url, true).Select(Json<T>.DeserializeAndClose);
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
        public static IObservable<TResponse> PostAndGetJson<TRequest, TResponse>(Uri url, TRequest postData)
            where TRequest : class where TResponse : class
        {
            var sequence = from postString in ObservableEx.DeferredStart(() => Json<TRequest>.Serialize(postData))
                           from completed in PostAndGetString(url, postString)
                           select Json<TResponse>.Deserialize(completed);
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
        public static IObservable<string> PostAndGetString(Uri url, string postData)
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

            /*
            var sequence = from client in Observable.Return(CreateClient(true))
                           from completed in Observable.CreateWithDisposable<UploadStringCompletedEventArgs>(
                               observer =>
                                   {
                                       var subscription =
                                           Observable.FromEvent<UploadStringCompletedEventArgs>(
                                               client, "UploadStringCompleted").Take(1).Select(e => e.EventArgs).
                                               Subscribe(observer);
#if DEBUG
                                       Debug.WriteLine("POST " + url.AbsoluteUri + "\r\n" + postData);
#endif
                                       client.Headers[HttpRequestHeader.ContentType] =
                                           "application/x-www-form-urlencoded";
                                       client.UploadStringAsync(url, postData);
                                       return subscription;
                                   }).TrySelect(evt => evt.Result)
                           select completed;
            return sequence;*/
        }

        private static void HandleMethod(object state) 
        {
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
        public static IObservable<TResponse> PostStringAndGetJson<TResponse>(Uri url, string postData)
            where TResponse : class
        {
            var sequence = from completed in PostAndGetString(url, postData)
                           select Json<TResponse>.Deserialize(completed);
            return sequence;
        }

        #endregion

        #region Methods

        private static IObservable<WebResponse> WebRequestAsync(Uri address, bool noCache)
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
        private static DisposableWebRequest CreateRequest(Uri address, bool noCache)
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

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private static WebClient CreateClient(bool noCache)
        {
            var client = new WebClient();
            client.Headers["X-Api-Key"] = "9abd1c4181d59dbece062455b941e64da474e5c7";

            if (IsAuthenticated)
            {
                client.Headers["X-User-Token"] = UserToken;
            }

            if (noCache)
            {
                client.Headers[HttpRequestHeader.Pragma] = "no-cache";
            }

            return client;
        }

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
        private static IObservable<TResult> TrySelect<T, TResult>(this IObservable<T> items, Func<T, TResult> selector)
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
                                if (webException.Response != null)
                                {
                                    using (var sr = new StreamReader(webException.Response.GetResponseStream()))
                                    {
                                        var response = Json<ResponseContract>.Deserialize(sr.ReadToEnd());
                                        if (response != null)
                                        {
                                            var newError = new ServiceException(response.Errors, webException, response.ResponseStatus);
                                            d.OnError(newError);
                                            return;
                                        }
                                    }
                                }

                                d.OnError(webException);

                            }
                            catch (Exception ex)
                            {
                                d.OnError(ex);
                            }
                        },
                    d.OnError,
                    d.OnCompleted));
        }

        #endregion

        public static void EnableGZip()
        {
            WebRequest.RegisterPrefix("http://", SharpGIS.WebRequestCreator.GZip);
            WebRequest.RegisterPrefix("https://", SharpGIS.WebRequestCreator.GZip);
        }
    }
}