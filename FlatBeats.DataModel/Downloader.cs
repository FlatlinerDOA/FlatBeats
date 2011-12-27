﻿//--------------------------------------------------------------------------------------------------
// <copyright file="Downloader.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.DataModel
{
    using System;
    using System.Diagnostics;
    using System.Net;

    using Microsoft.Phone.Reactive;

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
            var webRequest = from client in Observable.Return(CreateClient())
                             from completed in Observable.CreateWithDisposable<OpenReadCompletedEventArgs>(
                                 observer =>
                                     {
                                         var subscription =
                                             Observable.FromEvent<OpenReadCompletedEventArgs>(
                                                 client, "OpenReadCompleted").Take(1).Select(e => e.EventArgs).Subscribe
                                                 (observer);
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
                                                 return new Unit();
                                             })
                             select completed;

            return webRequest;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name = "T">
        /// </typeparam>
        /// <param name = "url">
        /// </param>
        /// <param name = "cacheFile">
        /// </param>
        /// <returns>
        /// </returns>
        public static IObservable<T> GetJson<T>(Uri url, string cacheFile = null) where T : class
        {
            IObservable<T> sequence = Observable.Empty<T>();
            if (cacheFile != null && Storage.Exists(cacheFile))
            {
                sequence =
                    Observable.Defer(() => Observable.Return(Storage.Load(cacheFile))).Select(
                        c => Json.Deserialize<T>(c)).Where(m => m != null);
            }

            var webRequest = from client in Observable.Return(CreateClient())
                             from completed in Observable.CreateWithDisposable<OpenReadCompletedEventArgs>(
                                 observer =>
                                     {
                                         var subscription =
                                             Observable.FromEvent<OpenReadCompletedEventArgs>(
                                                 client, "OpenReadCompleted").Take(1).Select(e => e.EventArgs).Subscribe
                                                 (observer);
#if DEBUG
                                         Debug.WriteLine("GET " + url.AbsoluteUri);
#endif
                                         client.OpenReadAsync(url);
                                         return subscription;
                                     }).TrySelect(evt => evt.Result)
                             select Json.DeserializeAndClose<T>(completed);
            sequence = sequence.Concat(
                webRequest.Do(
                    cache =>
                        {
                            if (cacheFile != null)
                            {
                                Storage.Save(cacheFile, Json.Serialize(cache));
                            }
                        }));
            return sequence;
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
            var sequence = from postString in ObservableEx.DeferredStart(() => Json.Serialize(postData))
                           from completed in PostAndGetString(url, postString)
                           select Json.Deserialize<TResponse>(completed);
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
            var sequence = from client in Observable.Return(CreateClient())
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
            return sequence;
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
                           select Json.Deserialize<TResponse>(completed);
            return sequence;
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private static WebClient CreateClient()
        {
            var client = new WebClient();
            client.Headers["X-Api-Key"] = "9abd1c4181d59dbece062455b941e64da474e5c7";

            if (IsAuthenticated)
            {
                client.Headers["X-User-Token"] = UserToken;
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
                            catch (Exception ex)
                            {
                                d.OnError(ex);
                            }
                        },
                    d.OnError,
                    d.OnCompleted));
        }

        #endregion
    }
}