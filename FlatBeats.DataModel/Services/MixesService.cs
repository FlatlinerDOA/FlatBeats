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

        public static IObservable<ReviewsResponseContract> GetMixReviewsAsync(string mixId, int pageNumber, int perPage)
        {
            return Downloader.GetJson<ReviewsResponseContract>(ApiUrl.MixReviews(mixId, pageNumber, perPage));
        }

        public static IObservable<MixesResponseContract> GetLatestMixesAsync()
        {
            return Downloader.GetJson<MixesResponseContract>(ApiUrl.LatestMixes()); //, LatestMixesCacheFile);
        }

        public static IObservable<MixesResponseContract> GetTagMixesAsync(string tag, string sort, int pageNumber, int perPage)
        {
            return Downloader.GetJson<MixesResponseContract>(ApiUrl.TaggedMixes(tag, sort, pageNumber, perPage));
        }

        public static IObservable<MixesResponseContract> GetSearchMixesAsync(string query, string sort, int pageNumber, int perPage)
        {
            return Downloader.GetJson<MixesResponseContract>(ApiUrl.SearchMixes(query, sort, pageNumber, perPage));
        }

        public static IObservable<MixContract> GetMixAsync(string mixId)
        {
            return from response in Downloader.GetJsonCachedAndRefreshed<MixResponseContract>(
                       ApiUrl.Mix(mixId), 
                       string.Format(MixCacheFile, mixId))
                   select response.Mix;
        }
    }
}
