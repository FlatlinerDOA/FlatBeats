namespace FlatBeats.DataModel.Services
{
    using System;

    using Microsoft.Phone.Reactive;

    public static class PlayerService
    {
        private const string NowPlayingFilePath = "nowplaying.json";
        
        public static void Stop()
        {
            Storage.Delete(NowPlayingFilePath);
        }
        
        public static void Save(this PlayingMixContract playing)
        {
            Storage.Save(NowPlayingFilePath, Json.Serialize(playing));
        }

        public static PlayingMixContract Load()
        {
            var data = Storage.Load(NowPlayingFilePath);
            return Json.Deserialize<PlayingMixContract>(data);
        }

        public static IObservable<PlayingMixContract> StartPlaying(this MixContract mix)
        {
            return from playToken in
                        Downloader.DownloadJson<PlayTokenResponseContract>(new Uri("http://8tracks.com/sets/new.json"))
                    let playFormat =
                        string.Format(
                            "http://8tracks.com/sets/{0}/play.json?mix_id={1}", playToken.PlayToken, mix.Id)
                    from response in Downloader.DownloadJson<PlayResponseContract>(new Uri(playFormat))
                    select new PlayingMixContract
                    {
                        PlayToken = playToken.PlayToken,
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
            return Downloader.DownloadJson<PlayResponseContract>(new Uri(nextFormat));
        }

        public static IObservable<PlayResponseContract> SkipToNextTrack(this PlayingMixContract playing)
        {
            var skipFormat = string.Format(
                "http://8tracks.com/sets/{0}/skip.json?mix_id={1}",
                playing.PlayToken,
                playing.MixId);
            return Downloader.DownloadJson<PlayResponseContract>(new Uri(skipFormat));
        }
    }
}
