namespace FlatBeats.DataModel
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using Microsoft.Phone.Reactive;

    /// <summary>
    /// 
    /// </summary>
    public static class Downloader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<T> GetJson<T>(Uri url, string cacheFile = null) where T : class 
        {
            IObservable<T> sequence = Observable.Empty<T>();
            if (cacheFile != null && Storage.Exists(cacheFile))
            {
                sequence = Observable.Start(() => Storage.Load(cacheFile))
                    .Select(Json.Deserialize<T>)
                    .Where(m => m != null);
            }

            var webRequest = from client in Observable.Start<WebClient>(CreateClient)
                             from completed in Observable.CreateWithDisposable<OpenReadCompletedEventArgs>(
                                 observer =>
                                     {
                                         var subscription =
                                             Observable.FromEvent<OpenReadCompletedEventArgs>(
                                                 client, "OpenReadCompleted").Take(1).Select(e => e.EventArgs).Subscribe
                                                 (observer);
                                         Debug.WriteLine("GET " + url.AbsoluteUri);
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
                    })).Take(1);
            return sequence;
        }

        public static IObservable<TResponse> PostAndGetJson<TRequest, TResponse>(Uri url, TRequest postData)
            where TRequest : class
            where TResponse : class
        {
            var sequence = from postString in Observable.Start(() => Json.Serialize(postData))
                           from completed in PostAndGetString(url, postString)
                           select Json.Deserialize<TResponse>(completed);
            return sequence;
        }


        public static IObservable<string> PostAndGetString(Uri url, string postData)
        {
            var sequence = from client in Observable.Start<WebClient>(CreateClient)
                           from completed in Observable.CreateWithDisposable<UploadStringCompletedEventArgs>(
                               observer =>
                               {
                                   var subscription =
                                       Observable.FromEvent<UploadStringCompletedEventArgs>(client, "UploadStringCompleted")
                                           .Take(1).Select(e => e.EventArgs).Subscribe(observer);
                                   client.UploadStringAsync(url, postData, "POST");
                                   return subscription;
                               }).TrySelect(evt => evt.Result)
                           select completed;
            return sequence;
        }


        private static string UserToken;

        public static void SetUserToken(string userToken)
        {
            UserToken = userToken;
        }

        public static bool IsAuthenticated 
        {
            get
            {
                return UserToken != null;
            } 
        }

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

        private static IObservable<TResult> TrySelect<T, TResult>(this IObservable<T> items, Func<T,TResult> selector)
        {
            return Observable.CreateWithDisposable<TResult>(
                d => items.Subscribe(
                    item =>
                        {
                            TResult result; 
                            try
                            {
                                result = selector(item);
                                d.OnNext(result);
                            } 
                            catch (Exception ex)
                            {
                                d.OnError(ex);
                            }
                        }, d.OnError, d.OnCompleted));
        }

        public static IObservable<TResponse> PostStringAndGetJson<TResponse>(Uri url, string postData)
            where TResponse : class
        {
            var sequence = from completed in PostAndGetString(url, postData)
                           select Json.Deserialize<TResponse>(completed);
            return sequence;
        }
    }
}
