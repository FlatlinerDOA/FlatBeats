
namespace FlatBeats.Tracks.DataModel
{
    using System;
    using System.Net;
    using System.Linq;
    using FlatBeats.DataModel.Services;

    public class YouTubeService
    {
        private readonly IAsyncDownloader downloader;

        public static readonly YouTubeService Instance = new YouTubeService();

        public YouTubeService() : this(AsyncDownloader.Instance)
        {
        }

        public YouTubeService(IAsyncDownloader downloader)
        {
            this.downloader = downloader;
        }

        public IObservable<EntryContract> SearchYouTube(string searchText, int maxResults)
        {
            var urlFormat = string.Format(
                "https://gdata.youtube.com/feeds/api/videos?alt=json&max-results={0}&q={1}", maxResults, HttpUtility.UrlEncode(searchText));
            return this.downloader.GetDeserializedAsync<VideoFeedContract>(new Uri(urlFormat, UriKind.Absolute));
        }
    }
}
