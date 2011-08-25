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
            //if (cacheFile != null && Storage.Exists(cacheFile))
            //{
            //    sequence = Observable.Start(() => Storage.Load(cacheFile))
            //        .Select(Json.Deserialize<T>)
            //        .Where(m => m != null);
            //}

            sequence = sequence.Concat(
                   from client in Observable.Start(() => new WebClient())
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
                       }).Select(evt => evt.Result) //.Select(result => Storage.Save(cacheFile, result))
                   select Json.DeserializeAndClose<T>(completed));
            return sequence;
        }
    }
}
