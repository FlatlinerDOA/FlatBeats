namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;

    using FlatBeats.DataModel.Profile;

    using Microsoft.Devices;
    using Microsoft.Phone.Reactive;

    public static class PlayerService
    {
        private const string PlayTokenFilePath = "playtoken.json";

        private const string NowPlayingFilePath = "nowplaying.json";

        private const string RecentlyPlayedFilePath = "recentmixes.json";

        public static IObservable<string> GetPlayToken()
        {
            return Downloader.GetJson<PlayTokenResponseContract>(
                new Uri("http://8tracks.com/sets/new.json"), 
                PlayTokenFilePath).Select(p => p.PlayToken);
        }

        public static void DeletePlayToken()
        {
            Storage.Delete(PlayTokenFilePath);
        }

        public static void Stop()
        {
            Storage.Delete(NowPlayingFilePath);
        }
        
        public static void ClearRecentlyPlayed()
        {
            Storage.Delete(RecentlyPlayedFilePath);
        }

        public static void SaveNowPlaying(this PlayingMixContract playing)
        {
            Storage.Save(NowPlayingFilePath, Json.Serialize(playing));
        }

        public static IObservable<MixesResponseContract> RecentlyPlayed()
        {
            return
                Observable.Start(() => Json.Deserialize<MixesResponseContract>(Storage.Load(RecentlyPlayedFilePath))).
                    Select(m => m ?? new MixesResponseContract() { Mixes = new List<MixContract>() });
        }

        public static IObservable<Unit> AddToRecentlyPlayed(MixContract mix)
        {
            string imageFilePath = "/Shared/Media/" + mix.Id + "-Original.jpg";

            return from recentlyPlayed in RecentlyPlayed().Do(
                       m =>
                        {
                            var duplicates = m.Mixes.Where(d => d.Id == mix.Id).ToList();
                            foreach (var duplicate in duplicates)
                            {
                                m.Mixes.Remove(duplicate);
                            }

                            m.Mixes.Insert(0, mix);

                            if (m.Mixes.Count > 10)
                            {
                                m.Mixes.Remove(m.Mixes.Last());
                            }
                        })
                   from mixes in Observable.Start(() => Storage.Save(RecentlyPlayedFilePath, Json.Serialize(recentlyPlayed)))
                       from save in Downloader.GetAndSaveFile(mix.CoverUrls.OriginalUrl, imageFilePath).Do(d =>
                       {
                           MediaHistoryItem item = new MediaHistoryItem();
                           item.Title = mix.Name;
                           item.ImageStream = Storage.LoadStream(imageFilePath);
                           MediaHistory.Instance.WriteRecentPlay(item);
                       })
                   select new Unit();
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
                   from added in AddToRecentlyPlayed(mix)
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
        
        public static IObservable<Unit> AddMixTrackHistory(int mixId, PlayResponseContract response)
        {
            // TODO : Load Mix Track History
            // Update and re-save
            return Observable.Empty<Unit>();
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
    }
}
