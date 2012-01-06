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
            this.NowPlaying.StopAsync(TimeSpan.Zero).ObserveOn(Scheduler.CurrentThread).Finally(this.NotifyComplete).Subscribe();
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
            this.NowPlaying.StopAsync(TimeSpan.Zero).ObserveOn(Scheduler.CurrentThread).Finally(
                () =>
                {
                    if (isFatal)
                    {
                        this.Abort();
                    }
                    else
                    {
                        this.NotifyComplete();
                    }
                }).Subscribe();
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
                    break;
                case PlayState.TrackEnded:
                    this.PlayNextTrackAsync(player).ObserveOn(Scheduler.CurrentThread).Finally(this.NotifyComplete).Subscribe();
                    return;
                case PlayState.Error:
                    this.NowPlaying.StopAsync(TimeSpan.Zero).ObserveOn(Scheduler.CurrentThread).Finally(this.NotifyComplete).Subscribe();
                    return;
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
                    this.StopPlayingAsync(player).Finally(this.NotifyComplete).Subscribe();
                    return;
                case UserAction.Pause:
                    player.Pause();
                    break;
                case UserAction.Play:
                    this.PlayTrackAsync(player).ObserveOn(Scheduler.CurrentThread).Finally(this.NotifyComplete).Subscribe();
                    return;
                case UserAction.SkipNext:
                    this.SkipToNextTrackAsync(player).ObserveOn(Scheduler.CurrentThread).Finally(this.NotifyComplete).Subscribe();
                    return;
            }

            this.NotifyComplete();
        }

        private void StopPlayingMix(BackgroundAudioPlayer player)
        {
            if (player.PlayerState != PlayState.Unknown)
            {
                player.Stop();
                player.Track = null;
            }
        }

        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        private IObservable<Unit> PlayNextTrackAsync(BackgroundAudioPlayer player)
        {
            if (this.NowPlaying == null || this.NowPlaying.Set == null ||  this.NowPlaying.Set.Track.TrackUrl == null)
            {
                return this.StopPlayingAsync(player);
            }

            var playNextTrack = from nextResponse in this.NowPlaying.NextTrackAsync(player)
                   where nextResponse != null && nextResponse.Status != null && nextResponse.Status.StartsWith("200")
                   from d in ObservableEx.DeferredStart(
                                   () =>
                                       {
                                           this.NowPlaying.Set = nextResponse.Set;
                                           this.NowPlaying.SaveNowPlaying();
                                       })
                               from play in this.PlayTrackAsync(player)
                               select play;

            return playNextTrack.OnErrorResumeNext(this.StopPlayingAsync(player));
        }

        private IObservable<Unit> StopPlayingAsync(BackgroundAudioPlayer player)
        {
            return this.NowPlaying.StopAsync(player).ObserveOn(Scheduler.CurrentThread).Do(_ => this.StopPlayingMix(player), ex => this.StopPlayingMix(player));
        }

        /// <summary>
        /// </summary>
        /// <param name="player">
        /// </param>
        private IObservable<Unit> PlayTrackAsync(BackgroundAudioPlayer player)
        {
            if (player.PlayerState == PlayState.Paused)
            {
                // If we're paused, we already have 
                // the track set, so just resume playing.
                player.Volume = 1;
                player.Play();
            }
            else
            {
                if (this.NowPlaying == null || this.NowPlaying.Set == null || this.NowPlaying.Set.Track == null || this.NowPlaying.Set.Track.TrackUrl == null)
                {
                    // Reset as we don't know what we're playing anymore.
                    return this.NowPlaying.StopAsync(player.Position).ObserveOn(Scheduler.CurrentThread).Do(_ => this.StopPlayingMix(player));
                }


                // Set which track to play. When the TrackReady state is received 
                // in the OnPlayStateChanged handler, call player.Play().
                return PlayerService.GetTrackAddressAsync(this.NowPlaying.Set.Track).ObserveOn(Scheduler.CurrentThread).Do(
                        trackUrl =>
                            {
                                var coverUrl = this.NowPlaying.Cover.ThumbnailUrl;
                                var playControls = !this.NowPlaying.Set.IsLastTrack && this.NowPlaying.Set.SkipAllowed
                                                       ? EnabledPlayerControls.Pause | EnabledPlayerControls.SkipNext | EnabledPlayerControls.FastForward
                                                       : EnabledPlayerControls.Pause;
                                var track = new AudioTrack(
                                    trackUrl,
                                    this.NowPlaying.Set.Track.Name,
                                    this.NowPlaying.Set.Track.Artist,
                                    this.NowPlaying.MixName,
                                    coverUrl,
                                    this.NowPlaying.MixId + "|" + this.NowPlaying.Set.Track.Id,
                                    playControls);
                                player.Track = track;
                                player.Volume = 1;
                            }).Select(_ => new Unit());
            }

            return Observable.Return(new Unit());
        }

        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        private IObservable<Unit> SkipToNextTrackAsync(BackgroundAudioPlayer player)
        {
            if (!this.NowPlaying.Set.SkipAllowed)
            {
                return Observable.Empty<Unit>();
            }

            var x = from skipResponse in this.NowPlaying.SkipToNextTrackAsync(player)
                    where skipResponse.Status.StartsWith("200")
                    select skipResponse;

            return from y in x.Do(
                response =>
                    {
                        this.NowPlaying.Set = response.Set;
                        this.NowPlaying.SaveNowPlaying();
                    }).ObserveOn(Scheduler.CurrentThread)
                   from play in this.PlayTrackAsync(player)
                   select play;
        }

        #endregion
    }
}