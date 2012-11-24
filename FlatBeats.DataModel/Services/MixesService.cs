namespace FlatBeats.DataModel.Services
{
    using System;

    using Flatliner.Phone;
    using Microsoft.Phone.Reactive;

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
            return Downloader.GetDeserializedAsync<MixesResponseContract>(ApiUrl.LatestMixes()).NotNull(); //, LatestMixesCacheFile);
        }

        public static IObservable<MixesResponseContract> GetHistoryMixesAsync()
        {
            return Downloader.GetDeserializedAsync<MixesResponseContract>(ApiUrl.HistoryMixes()).NotNull(); //, LatestMixesCacheFile);
        }

        public static IObservable<MixesResponseContract> GetTagMixesAsync(string tag, string sort, int pageNumber, int perPage)
        {
            return Downloader.GetDeserializedAsync<MixesResponseContract>(ApiUrl.TaggedMixes(tag, sort, pageNumber, perPage)).NotNull();
        }

        public static IObservable<MixesResponseContract> GetSearchMixesAsync(string query, string sort, int pageNumber, int perPage)
        {
            return Downloader.GetDeserializedAsync<MixesResponseContract>(ApiUrl.SearchMixes(query, sort, pageNumber, perPage)).NotNull();
        }

        public static IObservable<MixContract> GetMixAsync(string mixId)
        {
            return from response in Downloader.GetDeserializedCachedAndRefreshedAsync<MixResponseContract>(ApiUrl.Mix(mixId), string.Format(MixCacheFile, mixId)).NotNull()
                   where response.Mix != null
                   select response.Mix;
        }

        public static IObservable<TagsResponseContract> GetTagsAsync(int pageNumber)
        {
            var url = ApiUrl.Tags(pageNumber);
            return Downloader.GetDeserializedAsync<TagsResponseContract>(url);
        }
    }
}
