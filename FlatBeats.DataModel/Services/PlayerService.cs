namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using FlatBeats.DataModel.Profile;

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
            try
            {
                if (player.PlayerState != PlayState.Unknown && player.Track != null)
                {
                    playedDuration = player.Position;
                }
            }
            catch (InvalidOperationException)
            {
                // Background audio resources no longer available
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
                           try
                           {
                               using (var stream = Storage.ReadStream(imageFilePath))
                               {
                                   if (stream.Length >= 76800)
                                   {
                                       // TODO: Something else..?
                                       return;
                                   }

                                   var mediaHistoryItem = new MediaHistoryItem
                                                              {
                                                                  Title = mix.Name,
                                                                  ImageStream = stream
                                                              };
                                   mediaHistoryItem.PlayerContext.Add("MixId", mix.Id);
                                   MediaHistory.Instance.NowPlaying = mediaHistoryItem;
                                   stream.Close();
                               }

                               using (var secondStream = Storage.ReadStream(imageFilePath))
                               {
                                   if (secondStream.Length >= 76800)
                                   {
                                       // TODO: Something else..?
                                       return;
                                   }

                                   var item = new MediaHistoryItem { Title = mix.Name, ImageStream = secondStream };
                                   item.PlayerContext.Add("MixId", mix.Id);
                                   MediaHistory.Instance.WriteRecentPlay(item);
                                   secondStream.Close();
                               }
                           }
                           catch (COMException)
                           {
                               // No COM you can't stop the music.
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
                             from added in AddToRecentlyPlayedAsync(mix).Log("AddToRecentlyPlayedAsync: Done")

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
                    .Log("SetNowPlayingTileAsync: Done")
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
            BackgroundPinService.ResetApplicationTile();
        }

        public static IObservable<Unit> SetNowPlayingTileAsync(PlayingMixContract mix, string title, string backTitle)
        {
            if (!BackgroundPinService.IsApplicationPinnedToStart())
            {
                return Observable.Empty<Unit>();
            }

            return Observable.Defer(() => SaveFadedThumbnailAsync(mix)).SubscribeOnDispatcher().Try(
                url => BackgroundPinService.UpdateFlipTile(
                            title, 
                            backTitle, 
                            mix.MixName, 
                            mix.MixName, 
                            0, 
                            BackgroundPinService.DefaultTileUrl, 
                            url, 
                            url,
                            null,
                            new Uri(url.OriginalString.Replace(".jpg", ".wide.jpg"), UriKind.RelativeOrAbsolute), 
                            null)
                    ).Select(_ => new Unit());
        }

        private static IObservable<Uri> SaveFadedThumbnailAsync(PlayingMixContract mix)
        {
            ////var bitmap = new BitmapImage() { CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation };
            ////var opened = Observable.FromEvent<RoutedEventArgs>(bitmap, "ImageOpened").ToUnit().Take(1);
            ////var failed = from failure in Observable.FromEvent<ExceptionRoutedEventArgs>(bitmap, "ImageFailed").Take(1)
            ////             from result in Observable.Throw<Exception>(failure.EventArgs.ErrorException)
            ////             select new PortableUnit();


            Uri source;
            if (PlatformHelper.IsWindowsPhone78OrLater)
            {
                source = mix.Cover.OriginalUrl ?? mix.Cover.Max200Url ?? mix.Cover.ThumbnailUrl;
            }
            else
            {
                source = mix.Cover.ThumbnailUrl ?? mix.Cover.Max200Url;
            }

            if (source == null)
            {
                return Observable.Return<Uri>(null);
            }

            var result = Downloader.GetStreamAsync(source, false).ObserveOnDispatcher().Select(
                b =>
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.SetSource(b);
                        bitmap.CreateOptions = BitmapCreateOptions.None;

                        if (PlatformHelper.IsWindowsPhone78OrLater)
                        {
                            ResizeAndFade(bitmap, "NowPlaying.wide.jpg", 691, 336);
                        }
                        else
                        {
                            ResizeAndFade(bitmap, "NowPlaying.jpg", 173, 173);
                        }
                    }
                    catch (Exception)
                    {
                        // Gotta catch 'em all!
                    }

                    return source;
                });
            return result;

            ////return opened.Amb(failed).Select(
            ////    _ =>
            ////    {
            ////        if (PlatformHelper.IsWindowsPhone78OrLater)
            ////        {
            ////            ResizeAndFade(bitmap, "NowPlaying.wide.jpg", 691, 336);
            ////        }

            ////        return ResizeAndFade(bitmap, "NowPlaying.jpg", 173, 173);
            ////    });
        }

        private static Uri ResizeAndFade(BitmapImage bitmap, string fileName, int width, int height)
        {
            var img = new Image
                {
                    Source = bitmap,
                    Stretch = Stretch.UniformToFill,
                    Opacity = 0.70,
                    Width = width,
                    Height = height,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

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

            return new Uri("isostore:/Shared/ShellContent/" + fileName, UriKind.RelativeOrAbsolute);
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
                return AddToMixesPlayedTracks(playing.MixId, new List<TrackContract>() { playing.Set.Track }).ToUnit().Catch<PortableUnit, Exception>(ex => ObservableEx.SingleUnit());
            }


            var payment = from response in Downloader.GetDeserializedAsync<ResponseContract>(ApiUrl.ReportTrack(playing.PlayToken, playing.MixId, playing.Set.Track.Id))
                          from mixTrackList in AddToMixesPlayedTracks(playing.MixId, new List<TrackContract>() { playing.Set.Track})
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
            var playedTracks = from playToken in GetOrCreatePlayTokenAsync()
                               from response in Downloader.GetDeserializedAsync<PlayedTracksResponseContract>(ApiUrl.PlayedTracks(playToken, mix.Id))
                                .Coalesce(() => new PlayedTracksResponseContract
                                {
                                    Tracks = new List<TrackContract>()
                                })
                               from updatedList in AddToMixesPlayedTracks(mix.Id, response.Tracks)
                               select updatedList;
            return playedTracks;
        }

        public static IObservable<PlayedTracksResponseContract> AddToMixesPlayedTracks(this string mixId, IList<TrackContract> addTracks)
        {
            var cacheFile = string.Format("mixes/tracks-{0}.json", mixId);
            return from updatedList in Storage.LoadJsonAsync<PlayedTracksResponseContract>(cacheFile).Select(
                        cached =>
                            {
                                var trackList = cached ?? new PlayedTracksResponseContract
                                                        {
                                                            Tracks = new List<TrackContract>()
                                                        };
                                var preserved = addTracks.Concat(trackList.Tracks.Where(u => !addTracks.Any(t => u.Id == t.Id))).ToList();
                                trackList.Tracks = preserved;
                                return trackList;
                            })
                   from saved in Storage.SaveJsonAsync(cacheFile, updatedList)
                   select updatedList;
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
