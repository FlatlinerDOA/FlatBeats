namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using FlatBeats.DataModel.Profile;
    using FlatBeats.ViewModels;

    using Flatliner.Phone;

    using Microsoft.Devices;
    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;
    using Flatliner.Functional;

    public static class PlayerService
    {
        private static readonly IAsyncDownloader Downloader = AsyncDownloader.Instance;

        private static readonly IAsyncStorage Storage = AsyncIsolatedStorage.Instance;

        private const string PlayTokenFilePath = "playtoken.json";

        private const string NowPlayingFilePath = "nowplaying.json";

        private const string RecentlyPlayedFilePath = "recentmixes.json";

        public static IObservable<string> GetOrCreatePlayTokenAsync()
        {
            return Downloader.GetDeserializedCachedAsync<PlayTokenResponseContract>(
                new Uri("http://8tracks.com/sets/new.json", UriKind.Absolute), PlayTokenFilePath).Take(1).Select(p => p.PlayToken);
        }

        public static void DeletePlayToken()
        {
            Storage.Delete(PlayTokenFilePath);
        }

        public static IObservable<PortableUnit> StopAsync(this PlayingMixContract nowPlaying, BackgroundAudioPlayer player)
        {
            TimeSpan playedDuration = GetPlayedDuration(player);
            return nowPlaying.StopAsync(playedDuration);
        }

        private static TimeSpan GetPlayedDuration(BackgroundAudioPlayer player)
        {
            TimeSpan playedDuration = TimeSpan.Zero;
            if (player.PlayerState != PlayState.Unknown && player.Track != null)
            {
                playedDuration = player.Position;
            }

            return playedDuration;
        }

        public static IObservable<PortableUnit> StopAsync(this PlayingMixContract nowPlaying, TimeSpan timePlayed)
        {
            ResetNowPlayingTile();
            Storage.Delete(NowPlayingFilePath);
            
            if (nowPlaying == null)
            {
                return ObservableEx.SingleUnit();
            }

            return AddToMixTrackHistoryAsync(nowPlaying, timePlayed);
        }
        
        public static void ClearRecentlyPlayed()
        {
            Storage.Delete(RecentlyPlayedFilePath);
        }

        public static IObservable<PortableUnit> SaveNowPlayingAsync(this PlayingMixContract playing)
        {
            return Storage.SaveJsonAsync(NowPlayingFilePath, playing);
        }

        public static IObservable<MixesResponseContract> RecentlyPlayedAsync()
        {
            return Storage.LoadJsonAsync<MixesResponseContract>(RecentlyPlayedFilePath)
                .Select(m => m ?? new MixesResponseContract() { Mixes = new List<MixContract>() });
        }

        public static IObservable<PortableUnit> AddToRecentlyPlayedAsync(MixContract mix)
        {
            string imageFilePath = "/Shared/Media/" + mix.Id + "-Original.jpg";

            return from recentlyPlayed in RecentlyPlayedAsync().Do(
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
                   from mixes in Storage.SaveJsonAsync(RecentlyPlayedFilePath, recentlyPlayed)
                       from save in Downloader.GetAndSaveFileAsync(mix.Cover.ThumbnailUrl, imageFilePath, false).Do(d =>
                       {
                           using (var stream = Storage.ReadStream(imageFilePath))
                           {
                               var mediaHistoryItem = new MediaHistoryItem { Title = mix.Name, ImageStream = stream };
                               mediaHistoryItem.PlayerContext.Add("MixId", mix.Id);
                               MediaHistory.Instance.NowPlaying = mediaHistoryItem;
                               stream.Close();
                           }

                           using (var secondStream = Storage.ReadStream(imageFilePath))
                           {
                               var item = new MediaHistoryItem { Title = mix.Name, ImageStream = secondStream };
                               item.PlayerContext.Add("MixId", mix.Id);
                               MediaHistory.Instance.WriteRecentPlay(item);
                               secondStream.Close();
                           }
                       })
                   select ObservableEx.Unit;
        }

        public static bool NowPlayingExists()
        {
            return Storage.Exists(NowPlayingFilePath);
        }

        public static IObservable<PlayingMixContract> LoadNowPlayingAsync()
        {
            return Storage.LoadJsonAsync<PlayingMixContract>(NowPlayingFilePath);
        }

        public static IObservable<PlayingMixContract> StartPlayingAsync(this MixContract mix)
        {
            var playingMix = from playToken in GetOrCreatePlayTokenAsync()
                             from response in Downloader.GetDeserializedAsync<PlayResponseContract>(ApiUrl.Play(playToken, mix.Id))
                                 .Repeat(2)
                                 .Log("StartPlayingAsync: Attempting")
                                 .TakeFirst(ValidResponse)
                                 .Log("StartPlayingAsync: Valid Response")
                             from added in AddToRecentlyPlayedAsync(mix)
                             select new PlayingMixContract
                             {
                                 PlayToken = playToken,
                                 MixId = mix.Id,
                                 MixName = mix.Name,
                                 Cover = mix.Cover,
                                 Set = response.Set
                             };

            return from playing in playingMix
                   from tile in SetNowPlayingTileAsync(
                           playing, 
                           DataStrings.Title_ApplicationName, 
                           DataStrings.Title_NowPlaying)
                   select playing;
        }

        /// <summary>
        /// Gets the next similar mix, ensuring that nulls are never returned and that the same mix is never returned.
        /// </summary>
        /// <param name="mixId"></param>
        /// <returns></returns>
        public static IObservable<MixContract> GetNextMixAsync(string mixId)
        {
            return from playToken in GetOrCreatePlayTokenAsync()
                   from response in Downloader.GetDeserializedAsync<NextMixResponseContract>(ApiUrl.NextMix(playToken, mixId))
                   where response != null && response.NextMix != null && response.NextMix.Id != mixId
                   select response.NextMix;
        }

        public static void ResetNowPlayingTile()
        {
            PinHelper.ResetApplicationTile();
        }

        public static IObservable<Unit> SetNowPlayingTileAsync(PlayingMixContract mix, string title, string backTitle)
        {
            if (!PinHelper.IsApplicationPinnedToStart())
            {
                return Observable.Empty<Unit>();
            }

            return Observable.Defer(() => SaveFadedThumbnailAsync(mix)).SubscribeOnDispatcher().Do(
                url =>
                    {
                        PinHelper.UpdateFlipTile(
                            title, 
                            backTitle, 
                            mix.MixName, 
                            mix.MixName, 
                            0, 
                            PinHelper.DefaultTileUrl, 
                            PinHelper.ApplicationTileBackground, 
                            url,
                            null,
                            mix.Cover.OriginalUrl, 
                            null);
                        ////var newAppTile = new StandardTileData
                        ////{
                        ////    BackContent = mix.MixName,
                        ////    BackTitle = backTitle,
                        ////    BackBackgroundImage = url,
                        ////    BackgroundImage = new Uri("Background.png", UriKind.Relative),
                        ////    Title = title
                        ////};
                        ////appTile.Update(newAppTile);
                        
                    }).Select(_ => new Unit());
        }
        

        private static IObservable<Uri> SaveFadedThumbnailAsync(PlayingMixContract mix)
        {
            var bitmap = new BitmapImage();
            var opened = Observable.FromEvent<RoutedEventArgs>(bitmap, "ImageOpened").ToUnit().Take(1);
            var failed = from failure in Observable.FromEvent<ExceptionRoutedEventArgs>(bitmap, "ImageFailed").Take(1)
                         from result in Observable.Throw<Exception>(failure.EventArgs.ErrorException)
                         select new PortableUnit();

            bitmap.CreateOptions = BitmapCreateOptions.None;
            bitmap.UriSource = mix.Cover.ThumbnailUrl;
            return opened.Amb(failed).Select(
                _ =>
                {
                    return ResizeAndFade(bitmap, "NowPlaying.jpg", 173, 173);
                });
        }

        private static Uri ResizeAndFade(BitmapImage bitmap, string fileName, int width, int height)
        {
            var img = new Image() { Source = bitmap, Stretch = Stretch.Uniform, Opacity = 0.5, Width = width, Height = height };

            img.Measure(new Size(width, height));
            img.Arrange(new Rect(0, 0, width, height));
            var wb = new WriteableBitmap(width, height);
            wb.Render(img, new TranslateTransform());
            wb.Invalidate();
            using (var isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isolatedStorageFile.DirectoryExists("Images"))
                {
                    isolatedStorageFile.CreateDirectory("Images");
                }

                using (var file = isolatedStorageFile.CreateFile("/Shared/ShellContent/" + fileName))
                {
                    wb.SaveJpeg(file, width, height, 0, 85);
                    file.Flush();
                    file.Close();
                }
            }

            return new Uri("isostore:/Shared/ShellContent/NowPlaying.jpg", UriKind.RelativeOrAbsolute);
        }

        public static IObservable<PlayResponseContract> NextTrackAsync(this PlayingMixContract playing, BackgroundAudioPlayer player)
        {
            var time = GetPlayedDuration(player);
            return playing.NextTrackAsync(time);
        }

        private static IObservable<PlayResponseContract> NextTrackAsync(this PlayingMixContract playing, TimeSpan timePlayed)
        {
            return from addToHistory in AddToMixTrackHistoryAsync(playing, timePlayed)
                   from response in Downloader.GetDeserializedAsync<PlayResponseContract>(ApiUrl.NextTrack(playing.PlayToken, playing.MixId))
                        .Repeat(2)
                       .Log("NextTrackAsync: Attempting")
                       .TakeFirst(ValidResponse)
                       .Log("NextTrackAsync: ValidResponse")
                   select response;
        }

        private static bool ValidResponse(PlayResponseContract response)
        {
            return response != null && response.Status != null && response.Status.StartsWith("200")
            && ((response.Set.Track != null && response.Set.Track.TrackUrl != null) || response.Set.IsPastLastTrack);
        }

        public static IObservable<PortableUnit> AddToMixTrackHistoryAsync(this PlayingMixContract playing, TimeSpan timePlayed)
        {
            // If play duration was more than 30 seconds, post the report to Pay The Man
            if (timePlayed < TimeSpan.FromSeconds(30))
            {
                return ObservableEx.SingleUnit();
            }

            var payment = from response in Downloader.GetDeserializedAsync<ResponseContract>(ApiUrl.ReportTrack(playing.PlayToken, playing.MixId, playing.Set.Track.Id))
                          select new PortableUnit();

            return payment.Catch<PortableUnit, Exception>(ex => ObservableEx.SingleUnit());
        }

        public static IObservable<PlayResponseContract> SkipToNextTrackAsync(this PlayingMixContract playing, BackgroundAudioPlayer player)
        {
            var time = GetPlayedDuration(player);
            return playing.SkipToNextTrackAsync(time);
        }
    
        private static IObservable<PlayResponseContract> SkipToNextTrackAsync(this PlayingMixContract playing, TimeSpan timePlayed)
        {
            return from addToHistory in AddToMixTrackHistoryAsync(playing, timePlayed)
                   from response in Downloader.GetDeserializedAsync<PlayResponseContract>(ApiUrl.SkipTrack(playing.PlayToken, playing.MixId))
                   where ValidResponse(response)
                   select response;
        }

        public static IObservable<PlayedTracksResponseContract> PlayedTracksAsync(this MixContract mix)
        {
            var cacheFile = string.Format("mixes/tracks-{0}.json", mix.Id);
            var playedTracks = from playToken in GetOrCreatePlayTokenAsync()
                               from response in Downloader.GetDeserializedCachedAndRefreshedAsync<PlayedTracksResponseContract>(ApiUrl.PlayedTracks(playToken, mix.Id), cacheFile)
                               select response;
            return playedTracks;
        }

        public static IObservable<Uri> GetTrackAddressAsync(TrackContract track)
        {
            var remoteTrackUrl = new Uri(track.TrackUrl, UriKind.Absolute);
            var localFileName = "Track-" + track.Id + Path.GetExtension(track.TrackUrl);
            var localFullPath = "Audio/" + localFileName;
            if (Storage.Exists(localFullPath))
            {
                return Observable.Return(new Uri(localFullPath, UriKind.Relative));
            }

            ////return Downloader.GetAndSaveFile(remoteTrackUrl, localFullPath).Select(_ => new Uri(localFullPath, UriKind.Relative));
            return Observable.Return(remoteTrackUrl);
        }
    }
}
