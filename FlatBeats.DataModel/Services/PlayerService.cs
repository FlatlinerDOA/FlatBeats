namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Runtime.Serialization;

    using Microsoft.Phone.Reactive;

    public static class PlayerService
    {
        private const string PlayTokenFilePath = "playtoken.json";

        private const string NowPlayingFilePath = "nowplaying.json";
        
        public static IObservable<string> GetPlayToken()
        {
            return Downloader.GetJson<PlayTokenResponseContract>(
                new Uri("http://8tracks.com/sets/new.json"), PlayTokenFilePath).Select(p => p.PlayToken);
        }

        public static void Stop()
        {
            Storage.Delete(NowPlayingFilePath);
        }
        
        public static void SaveNowPlaying(this PlayingMixContract playing)
        {
            Storage.Save(NowPlayingFilePath, Json.Serialize(playing));
        }

        public static PlayingMixContract LoadNowPlaying()
        {
            var data = Storage.Load(NowPlayingFilePath);
            return Json.Deserialize<PlayingMixContract>(data);
        }

        public static IObservable<PlayingMixContract> StartPlaying(this MixContract mix)
        {
            return from playToken in GetPlayToken()
                    let playUrlFormat = string.Format("http://8tracks.com/sets/{0}/play.json?mix_id={1}", playToken, mix.Id)
                   from response in Downloader.GetJson<PlayResponseContract>(new Uri(playUrlFormat, UriKind.Absolute))
                    select new PlayingMixContract
                    {
                        PlayToken = playToken,
                        MixId = mix.Id,
                        MixName = mix.Name,
                        Cover = mix.CoverUrls,
                        Set = response.Set
                    };
        } 

        public static IObservable<PlayResponseContract> NextTrack(this PlayingMixContract playing)
        {
            var nextFormat = string.Format(
                "http://8tracks.com/sets/{0}/next.json?mix_id={1}",
                playing.PlayToken,
                playing.MixId);
            return Downloader.GetJson<PlayResponseContract>(new Uri(nextFormat, UriKind.Absolute));
        }

        public static IObservable<PlayResponseContract> SkipToNextTrack(this PlayingMixContract playing)
        {
            var skipFormat = string.Format(
                "http://8tracks.com/sets/{0}/skip.json?mix_id={1}",
                playing.PlayToken,
                playing.MixId);
            return Downloader.GetJson<PlayResponseContract>(new Uri(skipFormat, UriKind.Absolute));
        }

        public static IObservable<PlayedTracksResponseContract> PlayedTracks(this MixContract mix)
        {
            var playedTracks = from playToken in GetPlayToken()
                               let urlFormat = string.Format(
                                       "http://8tracks.com/sets/{0}/tracks_played.json?mix_id={1}",
                                       playToken,
                                       mix.Id)
                               from response in
                                   Downloader.GetJson<PlayedTracksResponseContract>(
                                       new Uri(urlFormat, UriKind.Absolute))
                               select response;
            return playedTracks;
        }

        public static IObservable<Unit> SetMixLiked(string mixId, bool isLiked)
        {
            var urlFormat = isLiked ? string.Format("http://8tracks.com/mixes/{0}/like.json", mixId) : string.Format("http://8tracks.com/mixes/{0}/unlike.json", mixId);
            return Downloader.PostStringAndGetJson<LikedMixResponseContract>(new Uri(urlFormat, UriKind.Absolute), string.Empty).Select(r => new Unit());
        }

        public static IObservable<Unit> SetTrackFavourite(string trackId, bool isFavourite)
        {
            var urlFormat = isFavourite ? string.Format("http://8tracks.com/tracks/{0}/fav.json", trackId) : string.Format("http://8tracks.com/tracks/{0}/unfav.json", trackId);
            return Downloader.PostStringAndGetJson<FavouritedTrackResponseContract>(new Uri(urlFormat, UriKind.Absolute), string.Empty).Select(r => new Unit());
        }
    }

    [DataContract]
    public class LikedMixResponseContract
    {
        [DataMember(Name = "mix")]
        public LikedMixContract Mix { get; set; }
    }

    [DataContract]
    public class LikedMixContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name ="liked_by_current_user")]
        public bool IsLiked { get; set; }
    }

    [DataContract]
    public class FavouritedTrackResponseContract
    {
        [DataMember(Name = "track")]
        public FavouritedTrackContract Track { get; set; }
    }

    [DataContract]
    public class FavouritedTrackContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "faved_by_current_user")]
        public bool IsFavourited { get; set; }
    }
}
