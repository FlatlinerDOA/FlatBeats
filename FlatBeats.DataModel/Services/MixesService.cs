namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Flatliner.Phone;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;

    public static class MixesService
    {
        private static readonly IAsyncDownloader Downloader = AsyncDownloader.Instance;

        private const string LatestMixesCacheFile = "/cache/latestmixes.json";

        private const string MixCacheFile = "/cache/mixes/{0}.json";

        public static IObservable<ReviewsResponseContract> GetMixReviewsAsync(string mixId, int pageNumber, int perPage)
        {
            return Downloader.GetDeserializedAsync<ReviewsResponseContract>(ApiUrl.MixReviews(mixId, pageNumber, perPage)).NotNull();
        }

        public static IObservable<MixesResponseContract> GetLatestMixesAsync()
        {
            return Downloader.GetDeserializedAsync<MixesResponseContract>(ApiUrl.LatestMixes()).Where(m => m != null); //, LatestMixesCacheFile);
        }

        public static IObservable<MixesResponseContract> GetTagMixesAsync(string tag, string sort, int pageNumber, int perPage)
        {
            return Downloader.GetDeserializedAsync<MixesResponseContract>(ApiUrl.TaggedMixes(tag, sort, pageNumber, perPage)).Where(m => m != null);
        }

        public static IObservable<MixesResponseContract> GetSearchMixesAsync(string query, string sort, int pageNumber, int perPage)
        {
            return Downloader.GetDeserializedAsync<MixesResponseContract>(ApiUrl.SearchMixes(query, sort, pageNumber, perPage)).Where(m => m != null);
        }

        public static IObservable<MixContract> GetMixAsync(string mixId)
        {
            return from response in Downloader.GetDeserializedCachedAndRefreshedAsync<MixResponseContract>(
                       ApiUrl.Mix(mixId), 
                       string.Format(MixCacheFile, mixId))
                   select response.Mix;
        }

        public static IObservable<TagsResponseContract> GetTagsAsync(int pageNumber)
        {
            return
                Downloader.GetDeserializedAsync<TagsResponseContract>(
                    new Uri("http://8tracks.com/all/mixes/tags.json?sort=recent&tag_page=" + pageNumber));
        }
    }
}
