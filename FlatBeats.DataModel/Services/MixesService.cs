namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;

    public static class MixesService
    {
        private const string LatestMixesCacheFile = "/cache/latestmixes.json";

        private const string MixCacheFile = "/cache/mixes/{0}.json";

        public static IObservable<ReviewsResponseContract> GetMixReviews(string mixId, int pageNumber, int perPage)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/mixes/{0}/reviews.json?&page={1}&per_page={2}", 
                mixId, 
                pageNumber, 
                perPage);
            return Downloader.GetJson<ReviewsResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<MixesResponseContract> GetLatestMixes()
        {
            return Downloader.GetJsonCachedAndRefreshed<MixesResponseContract>(
                    new Uri("http://8tracks.com/mixes.json", UriKind.RelativeOrAbsolute), LatestMixesCacheFile);
        }

        public static IObservable<MixesResponseContract> DownloadTagMixes(string tag, string sort, int pageNumber, int perPage)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?tag={0}&sort={1}&page={2}&per_page={3}", Uri.EscapeDataString(tag), sort, pageNumber, perPage),
                    UriKind.RelativeOrAbsolute));
        }

        public static IObservable<MixesResponseContract> DownloadSearchMixes(string query, string sort, int pageNumber, int perPage)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?q={0}&sort={1}&page={2}&per_page={3}", Uri.EscapeDataString(query), sort, pageNumber, perPage),
                    UriKind.RelativeOrAbsolute));
        }

        public static IObservable<MixContract> GetMixAsync(string mixId)
        {
            var mixUrl = new Uri(string.Format("http://8tracks.com/mixes/{0}.json", mixId), UriKind.RelativeOrAbsolute);
            return from response in Downloader.GetJsonCachedAndRefreshed<MixResponseContract>(mixUrl, string.Format(MixCacheFile, mixId))
                   select response.Mix;
        }


    }
}
