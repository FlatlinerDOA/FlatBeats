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

        public static IObservable<T> PostAndGetJson<T>(Uri url) where T : class
        {
            var sequence = from client in Observable.Start<WebClient>(CreateClient)
                           from completed in Observable.CreateWithDisposable<OpenWriteCompletedEventArgs>(
                               observer =>
                                   {
                                       var subscription =
                                           Observable.FromEvent<OpenWriteCompletedEventArgs>(client, "OpenWriteCompleted")
                                               .Take(1).Select(e => e.EventArgs).Subscribe(observer);
                                       client.OpenWriteAsync(url, "POST");
                                       return subscription;
                                   }).TrySelect(evt => evt.Result)
                           select Json.DeserializeAndClose<T>(completed);
            return sequence;
        }

        private static WebClient CreateClient()
        {
            var client = new WebClient();
            client.Headers["X-Api-Key"] = "9abd1c4181d59dbece062455b941e64da474e5c7";
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
    }
}
