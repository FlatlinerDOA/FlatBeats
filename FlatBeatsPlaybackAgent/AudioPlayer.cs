// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioPlayer.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System;

namespace FlatBeatsPlaybackAgent
{
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Devices;
    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class AudioPlayer : AudioPlayerAgent
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private PlayingMixContract nowPlaying;

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public PlayingMixContract NowPlaying
        {
            get
            {
                if (this.nowPlaying == null)
                {
                    this.nowPlaying = PlayerService.LoadNowPlaying();
                }

                return this.nowPlaying;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        protected override void OnCancel()
        {
            this.NotifyComplete();
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        /// <param name="track">
        /// The track that had the error
        /// </param>
        /// <param name="error">
        /// The error that occured
        /// </param>
        /// <param name="isFatal">
        /// If true, playback cannot continue and playback of the track will stop
        /// </param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        ///   itself has an unhandled exception, it won't get called back to handle its own errors.
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
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        /// <param name="track">
        /// The track playing at the time the playstate changed
        /// </param>
        /// <param name="playState">
        /// The new playstate of the player
        /// </param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        ///   caused the state change itself, assuming the application has opted-in to the callback
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            switch (playState)
            {
                case PlayState.TrackReady:
                    player.Play();
                    break;
                case PlayState.Stopped:
                    PlayerService.Stop();
                    break;
                case PlayState.TrackEnded:
                    this.PlayNextTrack(player);
                    break;
            }

            this.NotifyComplete();
        }

        /// <summary>
        /// Called when the user requests an action using system-provided UI and the application has requesed
        ///   notifications of the action
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        /// <param name="track">
        /// The track playing at the time of the user action
        /// </param>
        /// <param name="action">
        /// The action the user has requested
        /// </param>
        /// <param name="param">
        /// The data associated with the requested action.
        ///   In the current version this parameter is only for use with the Seek action,
        ///   to indicate the requested position of an audio track
        /// </param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        ///   for carrying out the user actions if they are supported
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
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        private void PlayNextTrack(BackgroundAudioPlayer player)
        {
            if (this.NowPlaying == null || this.NowPlaying.Set == null || this.NowPlaying.Set.IsLastTrack)
            {
                player.Stop();
                ////PlayerService.Stop();
                return;
            }

            var nextResponse = this.NowPlaying.NextTrack().First();
            if (nextResponse.Status.StartsWith("200"))
            {
                this.NowPlaying.Set = nextResponse.Set;
                this.NowPlaying.SaveNowPlaying();
                this.PlayTrack(player);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="player">
        /// </param>
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
                if (this.NowPlaying == null || this.NowPlaying.Set == null || this.NowPlaying.Set.Track == null || this.NowPlaying.Set.Track.TrackUrl == null)
                {
                    player.Stop();
                    return;
                }

                var trackUrl = new Uri(this.NowPlaying.Set.Track.TrackUrl, UriKind.Absolute);
                var coverUrl = this.NowPlaying.Cover.ThumbnailUrl;

                var track = new AudioTrack(
                    trackUrl, 
                    this.NowPlaying.Set.Track.Name, 
                    this.NowPlaying.Set.Track.Artist, 
                    this.NowPlaying.MixName, 
                    coverUrl, 
                    this.NowPlaying.MixId + "|" + this.NowPlaying.Set.Track.Id, 
                    EnabledPlayerControls.Pause | EnabledPlayerControls.SkipNext);
                player.Track = track;
            }
        }

        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        private void SkipToNextTrack(BackgroundAudioPlayer player)
        {
            if (!this.NowPlaying.Set.SkipAllowed)
            {
                return;
            }

            var skipResponse = this.NowPlaying.SkipToNextTrack().First();
            if (skipResponse.Status.StartsWith("200"))
            {
                this.NowPlaying.Set = skipResponse.Set;
                this.NowPlaying.SaveNowPlaying();
                this.PlayTrack(player);
            }
        }

        #endregion
    }
}