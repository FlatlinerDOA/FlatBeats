namespace EightTracks.DataModel
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
            if (cacheFile != null && Storage.Exists(cacheFile))
            {
                sequence = Observable.Start(() => Storage.Load(cacheFile))
                    .Select(Json.Deserialize<T>)
                    .Where(m => m != null);
            }

            sequence = sequence.Concat(
                   from client in Observable.Return(new WebClient())
                   from completed in Observable.CreateWithDisposable<DownloadStringCompletedEventArgs>(
                       observer =>
                       {
                           var subscription = Observable.FromEvent<DownloadStringCompletedEventArgs>(
                                   client, "DownloadStringCompleted")
                                   .Take(1)
                                   .Select(e => e.EventArgs)
                                   .Subscribe(observer);
                           client.DownloadStringAsync(url);
                           return subscription;
                       }).Do(evt => Storage.Save(cacheFile, evt.Result))
                   select Json.Deserialize<T>(completed.Result));
            return sequence;
        }
    }
}
