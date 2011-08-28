namespace FlatBeats.DataModel
{
    using System;
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
        public static IObservable<T> DownloadJson<T>(Uri url, string cacheFile = null) where T : class 
        {
            IObservable<T> sequence = Observable.Empty<T>();
            ////if (cacheFile != null && Storage.Exists(cacheFile))
            ////{
            ////    sequence = Observable.Start(() => Storage.Load(cacheFile))
            ////        .Select(Json.Deserialize<T>)
            ////        .Where(m => m != null);
            ////}

            sequence = sequence.Concat(
                   from client in Observable.Start<WebClient>(CreateClient)
                   from completed in Observable.CreateWithDisposable<OpenReadCompletedEventArgs>(
                       observer =>
                       {
                           var subscription = Observable.FromEvent<OpenReadCompletedEventArgs>(
                                   client, "OpenReadCompleted")
                                   .Take(1)
                                   .Select(e => e.EventArgs)
                                   .Subscribe(observer);
                           client.OpenReadAsync(url);
                           return subscription;
                       }).TrySelect(evt => evt.Result) ////.Select(result => Storage.Save(cacheFile, result))
                   select Json.DeserializeAndClose<T>(completed));
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
