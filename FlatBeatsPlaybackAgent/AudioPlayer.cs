using System;
using Microsoft.Phone.BackgroundAudio;

namespace FlatBeatsPlaybackAgent
{
    using System.IO.IsolatedStorage;

    using FlatBeats.DataModel;

    using Microsoft.Devices;
    using Microsoft.Phone.Reactive;

    public class AudioPlayer : AudioPlayerAgent
    {
        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            switch (playState)
            {
                case PlayState.TrackReady:
                    player.Play();
                    break;
                case PlayState.Stopped:
                    player.Stop();
                    ////Storage.Delete("nowplaying.json");
                    break;
                case PlayState.TrackEnded:
                    this.PlayNextTrack(player);
                    break;
            }

            this.NotifyComplete();
        }


        /// <summary>
        /// Called when the user requests an action using system-provided UI and the application has requesed
        /// notifications of the action
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            switch (action)
            {
                case UserAction.Stop:
                    player.Stop();
                    break;
                case UserAction.Pause:
                    player.Pause();
                    break;
                case UserAction.Play:
                    this.PlayTrack(player);
                    break;
                case UserAction.SkipNext:
                    this.SkipToNextTrack(player);
                    break;
            }

            this.NotifyComplete();
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            if (isFatal)
            {
                this.Abort();
            }
            else
            {
                this.NotifyComplete();
            }
        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        protected override void OnCancel()
        {
            this.NotifyComplete();
        }

        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        private void PlayNextTrack(BackgroundAudioPlayer player)
        {
            if (this.NowPlaying.Set.IsLastTrack)
            {
                player.Stop();
                return;
            }

            var nextFormat = string.Format(
                "http://8tracks.com/sets/{0}/next.json?mix_id={1}",
                this.NowPlaying.PlayToken,
                this.NowPlaying.MixId);
            var nextResponse = Downloader.DownloadJson<PlayResponseContract>(new Uri(nextFormat)).First();
            if (nextResponse.Status.StartsWith("200"))
            {
                this.NowPlaying.Set = nextResponse.Set;
                this.Save();
                this.PlayTrack(player);
            }
        }

        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        private void SkipToNextTrack(BackgroundAudioPlayer player)
        {
            if (!this.NowPlaying.Set.SkipAllowed)
            {
                return;
            }

            var skipFormat = string.Format(
                "http://8tracks.com/sets/{0}/skip.json?mix_id={1}", 
                this.NowPlaying.PlayToken, 
                this.NowPlaying.MixId);
            var skipResponse = Downloader.DownloadJson<PlayResponseContract>(new Uri(skipFormat)).First();
            if (skipResponse.Status.StartsWith("200"))
            {
                this.NowPlaying.Set = skipResponse.Set;
                this.Save();
                this.PlayTrack(player);
            }
        }

        private void PlayTrack(BackgroundAudioPlayer player)
        {
            if (PlayState.Paused == player.PlayerState)
            {
                // If we're paused, we already have 
                // the track set, so just resume playing.
                player.Play();
            }
            else
            {
                // Set which track to play. When the TrackReady state is received 
                // in the OnPlayStateChanged handler, call player.Play().
                var trackUrl = new Uri(this.NowPlaying.Set.Track.TrackUrl, UriKind.Absolute);
                var coverUrl = this.NowPlaying.Cover.ThumbnailUrl;

                ////MediaHistoryItem mediaHistoryItem = new MediaHistoryItem();

                //////<hubTileImageStream> must be a valid ImageStream.
                ////mediaHistoryItem.ImageStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(); 
                ////mediaHistoryItem.Source = "";
                ////mediaHistoryItem.Title = "RecentPlay";
                ////mediaHistoryItem.PlayerContext.Add("keyString", "Song Name");
                ////MediaHistory.Instance.WriteRecentPlay(mediaHistoryItem);

                player.Track = new AudioTrack(
                    trackUrl, 
                    this.NowPlaying.Set.Track.Name, 
                    this.NowPlaying.Set.Track.Artist, 
                    this.NowPlaying.MixName, 
                    coverUrl);
            }
        }

        private void Save()
        {
            Storage.Save("Shared/Media/nowplaying.json", Json.Serialize(this.NowPlaying));
        }

        private PlayingMixContract nowPlaying;

        public PlayingMixContract NowPlaying
        {
            get
            {
                if (this.nowPlaying == null)
                {
                    var data = Storage.Load("Shared/Media/nowplaying.json");
                    this.nowPlaying = Json.Deserialize<PlayingMixContract>(data);
                }

                return this.nowPlaying;
            }
        }
    }
}
