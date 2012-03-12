namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Net;

    public static class ApiUrl
    {
        /// <summary>
        /// 8tracks.com host address
        /// </summary>
        private const string Host = "http://8tracks.com";

        /// <summary>
        /// 8tracks.com api version number
        /// </summary>
        private const string ApiVersion = "2";
        
        public static Uri Mix(string mixId)
        {
            var mixUrl = string.Format(
                "http://8tracks.com/mixes/{0}.json?api_version={1}", 
                mixId, 
                ApiVersion);
            return new Uri(mixUrl, UriKind.Absolute);
        }        
        
        public static Uri TaggedMixes(string tag, string sort, int pageNumber, int perPage)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/mixes.json?tag={0}&sort={1}&page={2}&per_page={3}&api_version={4}", 
                ApiUrl.Escape(tag), 
                sort, 
                pageNumber, 
                perPage, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri SearchMixes(string query, string sort, int pageNumber, int perPage)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/mixes.json?q={0}&sort={1}&page={2}&per_page={3}&api_version={4}", 
                ApiUrl.Escape(query), 
                sort, 
                pageNumber, 
                perPage, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri LatestMixes()
        {
            var urlFormat = string.Format("http://8tracks.com/mixes.json?api_version={0}", ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri MixReviews(string mixId, int pageNumber, int perPage)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/mixes/{0}/reviews.json?&page={1}&per_page={2}&api_version={3}",
                mixId,
                pageNumber,
                perPage, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri UserProfile(string userId)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/users/{0}.json?api_version={1}", 
                userId, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri UserMixes(string userId, string view, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/users/{0}/mixes.json?view={1}&page={2}&per_page={3}&api_version={4}", 
                userId, 
                view, 
                pageNumber, 
                pageSize, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri UserMixes(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/users/{0}/mixes.json?page={1}&per_page={2}&api_version={3}", 
                userId, 
                pageNumber, 
                pageSize, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        /// <summary>
        /// Gets the url https://8tracks.com/sessions.json
        /// </summary>
        public static Uri Authenticate()
        {
            var urlFormat = string.Format("https://8tracks.com/sessions.json?api_version={0}", ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static string Escape(string text)
        {
            return HttpUtility.UrlEncode(text).Replace(".", "%2E");
        }

        public static Uri NextTrack(string playToken, string mixId)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/sets/{0}/next.json?mix_id={1}&skip_aac_v2=1&api_version={2}",
                playToken,
                mixId, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri NextMix(string playToken, string mixId)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/sets/{0}/next_mix.json?mix_id={1}&api_version={2}", 
                playToken, 
                mixId,
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri Play(string playToken, string mixId)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/sets/{0}/play.json?mix_id={1}&skip_aac_v2=1&api_version={2}", 
                playToken, 
                mixId, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri SkipTrack(string playToken, string mixId)
        {
            var skipFormat = string.Format(
                "http://8tracks.com/sets/{0}/skip.json?mix_id={1}&skip_aac_v2=1&api_version={2}",
                playToken,
                mixId, 
                ApiVersion);
           return new Uri(skipFormat, UriKind.Absolute);
        }

        public static Uri PlayedTracks(string playToken, string mixId)
        {
            var urlFormat = string.Format(
                "http://8tracks.com/sets/{0}/tracks_played.json?mix_id={1}&api_version={2}",
                playToken,
                mixId, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }

        public static Uri ReportTrack(string playToken, string mixId, string trackId)
        {
            var urlFormat = string.Format(
                @"http://8tracks.com/sets/{0}/report.json?track_id={1}&mix_id={2}&api_version={3}",
                playToken,
                trackId,
                mixId, 
                ApiVersion);
            return new Uri(urlFormat, UriKind.Absolute);
        }
    }
}
