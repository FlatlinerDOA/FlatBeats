﻿namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using FlatBeats.DataModel.Profile;

    using Microsoft.Devices;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;

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
            ResetNowPlayingTile();
            Storage.Delete(NowPlayingFilePath);
        }
        
        public static void ClearRecentlyPlayed()
        {
            Storage.Delete(RecentlyPlayedFilePath);
        }

        public static void SaveNowPlaying(this PlayingMixContract playing)
        {
            Storage.Save(NowPlayingFilePath, Json.Serialize(playing));
            SetNowPlayingTile(playing, StringResources.Title_ApplicationName, StringResources.Title_NowPlaying);
        }

        public static IObservable<MixesResponseContract> RecentlyPlayed()
        {
            return Observable.Start(() => Json.Deserialize<MixesResponseContract>(Storage.Load(RecentlyPlayedFilePath))).
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
                       from save in Downloader.GetAndSaveFile(mix.Cover.ThumbnailUrl, imageFilePath).Do(d =>
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
                       Cover = mix.Cover,
                       Set = response.Set
                   };
        }

        public static void ResetNowPlayingTile()
        {
            var appTile = ShellTile.ActiveTiles.Where(tile => tile.NavigationUri == new Uri("/", UriKind.Relative)).FirstOrDefault();
            if (appTile == null)
            {
                return;
            }

            var newAppTile = new StandardTileData()
            {
                BackgroundImage = new Uri("Background.png", UriKind.Relative),
                Title = "Flat Beats"
            };
            appTile.Update(newAppTile);
        }

        public static IObservable<Unit> SetNowPlayingTile(PlayingMixContract mix, string title, string backTitle)
        {
            var appTile = ShellTile.ActiveTiles.Where(tile => tile.NavigationUri == new Uri("/", UriKind.Relative)).FirstOrDefault();
            if (appTile == null)
            {
                return Observable.Empty<Unit>();
            }

            return Observable.Defer(() => SaveFadedThumbnail(mix)).SubscribeOnDispatcher().Do(
                url =>
                    {
                        var newAppTile = new StandardTileData
                            {
                                BackContent = mix.MixName,
                                BackTitle = backTitle,
                                BackBackgroundImage = url,
                                BackgroundImage = new Uri("Background.png", UriKind.Relative),
                                Title = title 
                            };
                        appTile.Update(newAppTile);
                    }).Select(_ => new Unit());
        }
        

        private static IObservable<Uri> SaveFadedThumbnail(PlayingMixContract mix)
        {
            var bitmap = new BitmapImage();
            var opened = Observable.FromEvent<RoutedEventArgs>(bitmap, "ImageOpened").Select(_ => new Unit()).Take(1);
            var failed = from failure in Observable.FromEvent<ExceptionRoutedEventArgs>(bitmap, "ImageFailed").Take(1)
                         from result in Observable.Throw<Exception>(failure.EventArgs.ErrorException)
                         select new Unit();

            bitmap.CreateOptions = BitmapCreateOptions.None;
            bitmap.UriSource = mix.Cover.ThumbnailUrl;
            return opened.Amb(failed).Select(
                _ =>
                {
                    var img = new Image()
                    {
                        Source = bitmap,
                        Stretch = Stretch.Uniform,
                        Opacity = 0.5,
                        Width = 173,
                        Height = 173
                    };

                    img.Measure(new Size(173, 173));
                    img.Arrange(new Rect(0, 0, 173, 173));
                    var wb = new WriteableBitmap(173, 173);
                    wb.Render(img, new TranslateTransform());
                    wb.Invalidate();
                    using (var isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!isolatedStorageFile.DirectoryExists("Images"))
                        {
                            isolatedStorageFile.CreateDirectory("Images");
                        }

                        using (var file = isolatedStorageFile.CreateFile("/Shared/ShellContent/NowPlaying.jpg"))
                        {
                            wb.SaveJpeg(file, 173, 173, 0, 85);
                            file.Close();
                        }
                    }

                    return new Uri("isostore:/Shared/ShellContent/NowPlaying.jpg", UriKind.RelativeOrAbsolute);
                });
        }

        public static IObservable<PlayResponseContract> NextTrack(this PlayingMixContract playing, TimeSpan timePlayed)
        {
            var nextFormat = string.Format(
                "http://8tracks.com/sets/{0}/next.json?mix_id={1}",
                playing.PlayToken,
                playing.MixId);
            return from addToHistory in AddToMixTrackHistory(playing, timePlayed)
                   from response in Downloader.GetJson<PlayResponseContract>(new Uri(nextFormat, UriKind.Absolute))
                   select response;
        }

        private static IObservable<Unit> AddToMixTrackHistory(PlayingMixContract playing, TimeSpan timePlayed)
        {
            // If play duration was more than 30 seconds, post the report to Pay The Man
            if (timePlayed < TimeSpan.FromSeconds(30))
            {
                return Observable.Return(new Unit());
            }

            var payment = from response in
                       Downloader.GetJson<ResponseContract>(
                           new Uri(
                       string.Format(
                           @"http://8tracks.com/sets/{0}/report.json?track_id={1}&mix_id={2}",
                           playing.PlayToken,
                           playing.Set.Track.Id,
                           playing.MixId),
                       UriKind.Absolute))
                   select new Unit();

            return payment.OnErrorResumeNext(Observable.Return(new Unit()));
        }

        public static IObservable<PlayResponseContract> SkipToNextTrack(this PlayingMixContract playing, TimeSpan timePlayed)
        {
            var skipFormat = string.Format(
                "http://8tracks.com/sets/{0}/skip.json?mix_id={1}",
                playing.PlayToken,
                playing.MixId);
            return from addToHistory in AddToMixTrackHistory(playing, timePlayed)
                   from response in Downloader.GetJson<PlayResponseContract>(new Uri(skipFormat, UriKind.Absolute))
                   select response;
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
