//--------------------------------------------------------------------------------------------------
// <copyright file="AudioPlayer.cs" company="Andrew Chisholm">
//   Copyright (c) 2012 Andrew Chisholm. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeatsPlaybackAgent
{
    using System;
    using System.Diagnostics;
    using System.Net;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Profile;
    using FlatBeats.DataModel.Services;

    using Flatliner.Phone;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Net.NetworkInformation;
    using Microsoft.Phone.Reactive;
    using Flatliner.Functional;

    /// <summary>
    /// </summary>
    public class AudioPlayer : AudioPlayerAgent
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// </summary>
        private CompositeDisposable lifetime;

        /// <summary>
        /// </summary>
        private volatile PlayingMixContract nowPlaying;

        /// <summary>
        /// </summary>
        private SettingsContract userSettings;

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public PlayingMixContract NowPlaying
        {
            get
            {
                return this.nowPlaying;
            }

            set
            {
                lock (this.syncRoot)
                {
                    this.nowPlaying = value;
                }
            }
        }

        private IObservable<PlayingMixContract> LoadNowPlayingAsync()
        {
            return from settings in ProfileService.Instance.GetSettingsAsync().Do(s => this.userSettings = s)
                   from nowPlaying in PlayerService.LoadNowPlayingAsync().Do(v => this.NowPlaying = v)
                   select nowPlaying;
        } 
        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public SettingsContract UserSettings
        {
            get
            {
                return this.userSettings;
            }
        }

        /// <summary>
        /// </summary>
        private CompositeDisposable Lifetime
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.lifetime ?? (this.lifetime = new CompositeDisposable());
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        protected override void OnCancel()
        {
            this.Completed();
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
                player.Track = null;
                var stopProcess = from _ in this.LoadNowPlayingAsync()
                                  from stop in this.NowPlaying.StopAsync(TimeSpan.Zero).ObserveOn(Scheduler.CurrentThread)
                                  select stop;
                this.Lifetime.Add(stopProcess.Finally(this.Completed).Subscribe(
                    _ => { },
                    ex => this.ReportFatalStopError(ex, error)));
            }
        }

        private void ReportFatalStopError(Exception stopError, Exception originalError)
        {
            LittleWatsonLog.ReportException(stopError, "Errored trying to handle another error: " + originalError);
            this.Abort();
            
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
                    this.Lifetime.Add(
                        (from _ in this.LoadNowPlayingAsync()
                        from t in this.PlayNextTrackAsync(player)
                        select t).ObserveOn(Scheduler.CurrentThread).Finally(this.Completed).Subscribe(
                            _ => { },
                            ex => this.ReportFatalStopError(ex, null)));
                    return;
                case PlayState.Error:
                    this.Lifetime.Add(
                        (from _ in this.LoadNowPlayingAsync()
                         from stop in this.NowPlaying.StopAsync(TimeSpan.Zero)
                         select stop).ObserveOn(Scheduler.CurrentThread).Finally(this.Completed).Subscribe(
                            _ => { },
                            ex => this.ReportFatalStopError(ex, null)));
                    return;
            }

            this.Completed();
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
        protected override void OnUserAction(
            BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            switch (action)
            {
                case UserAction.Stop:
                    try
                    {
                        if (player.PlayerState != PlayState.Paused || !PlayerService.NowPlayingExists())
                        {
                            this.Lifetime.Add((from _ in this.LoadNowPlayingAsync()
                                               from __ in this.StopPlayingAsync(player)
                                               select __).Finally(this.Completed).Subscribe());
                            return;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Uh oh background resources not available.
                    }

                    break;
                case UserAction.Pause:
                    try
                    {
                        player.Pause();
                    }
                    catch (InvalidOperationException)
                    {
                        // Probably no track.. just continue on our merry way
                    }

                    break;
                case UserAction.Play:
                    this.Lifetime.Add((from _ in this.LoadNowPlayingAsync()
                                       from __ in this.PlayTrackAsync(player)
                                       select __).ObserveOn(Scheduler.CurrentThread).Finally(this.Completed).Subscribe(_ => { },
                        ex => this.ReportFatalStopError(ex, null)));

                    return;
                case UserAction.SkipNext:
                    this.Lifetime.Add((from _ in this.LoadNowPlayingAsync()
                                       from __ in this.SkipToNextTrackAsync(player)
                                       select __).ObserveOn(Scheduler.CurrentThread).Finally(this.Completed).Subscribe(_ => { },
                        ex => this.ReportFatalStopError(ex, null)));

                    return;
            }

            this.Completed();
        }

        /// <summary>
        /// </summary>
        private void Completed()
        {
            this.DisposeLifetime();
            this.NotifyComplete();
        }

        /// <summary>
        /// </summary>
        private void DisposeLifetime()
        {
            lock (this.syncRoot)
            {
                if (this.lifetime == null)
                {
                    return;
                }

                this.lifetime.Dispose();
                this.lifetime = null;
            }
        }

        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        private IObservable<PortableUnit> PlayNextTrackAsync(BackgroundAudioPlayer player)
        {
            Debug.WriteLine("Player: PlayNextTrackAsync");

            if (this.NowPlaying == null || this.NowPlaying.Set == null)
            {
                Debug.WriteLine("Player: PlayNextTrackAsync (Now Playing not set)");
                return this.StopPlayingAsync(player);
            }

            if (this.UserSettings.PlayOverWifiOnly && NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
            {
                return this.StopPlayingAsync(player);
            }

            ////  
            if (this.NowPlaying.Set.IsLastTrack || this.NowPlaying.Set.IsPastLastTrack)
            {
                if (this.UserSettings.PlayNextMix)
                {
                    var currentMixId = this.NowPlaying.MixId;
                    var nextMix = from ignoreLastTrack in this.NowPlaying.NextTrackAsync(player).DefaultIfEmpty()
                                  from stop in this.NowPlaying.StopAsync(player)
                                  from mix in PlayerService.GetNextMixAsync(currentMixId)
                                  from start in mix.StartPlayingAsync().Do(
                                       t => 
                                       { 
                                           this.NowPlaying = t; 
                                       })
                                   from _ in this.NowPlaying.SaveNowPlayingAsync()
                                   from play in this.PlayTrackAsync(player)
                                   select play;
                    return nextMix
                        .Catch<PortableUnit, ServiceException>(ex => this.StopPlayingAsync(player))
                        .Catch<PortableUnit, WebException>(ex => this.StopPlayingAsync(player));
                }

                return this.StopPlayingAsync(player);
            }

            var playNextTrack = from nextResponse in this.NowPlaying.NextTrackAsync(player).Do(r => 
                                        {
                                            this.NowPlaying.Set = r.Set;
                                        })
                                from _ in this.NowPlaying.SaveNowPlayingAsync()
                                from play in this.PlayTrackAsync(player)
                                select play;

            return playNextTrack
                .Catch<PortableUnit, ServiceException>(ex => this.StopPlayingAsync(player))
                .Catch<PortableUnit, WebException>(ex => this.StopPlayingAsync(player))
                .Catch<PortableUnit, Exception>(
                ex =>
                    {
                        var data = "Error playing next track, we have to stop!\r\n";
                        if (this.NowPlaying != null)
                        {
                            data += Json<PlayingMixContract>.Instance.SerializeToString(this.NowPlaying);
                        }

                        LittleWatsonLog.ReportException(ex, data);
                        Debug.WriteLine("Player: PlayNextTrackAsync (Playback Error, stopping!)");
                        return this.StopPlayingAsync(player);
                    });
        }

        /// <summary>
        /// </summary>
        /// <param name="player">
        /// </param>
        private IObservable<PortableUnit> PlayTrackAsync(BackgroundAudioPlayer player)
        {
            if (this.UserSettings.PlayOverWifiOnly && NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
            {
                return this.StopPlayingAsync(player);
            }

            if (this.NowPlaying == null || this.NowPlaying.Set == null)
            {
                Debug.WriteLine("Player: PlayTrackAsync (Now Playing not set)");

                // Reset as we don't know what we're playing anymore.
                return this.StopPlayingAsync(player);
            }

            try
            {
                if (player.PlayerState == PlayState.Paused && player.Track.Tag.StartsWith(this.NowPlaying.MixId + "|"))
                {
                    Debug.WriteLine("Player: PlayTrackAsync (Resume from Paused)");

                    // If we're paused, we already have 
                    // the track set, so just resume playing.
                    player.Volume = 1;
                    player.Play();

                    return ObservableEx.SingleUnit();
                }
            }
            catch (InvalidOperationException)
            {
                // Background audio resources not available
                return ObservableEx.SingleUnit();
            }

            if (this.NowPlaying.Set.Track == null || this.NowPlaying.Set.Track.TrackUrl == null)
            {
                return this.StopPlayingAsync(player);
            }

            Debug.WriteLine("Player: PlayTrackAsync (Playing)");

            // Set which track to play. When the TrackReady state is received 
            // in the OnPlayStateChanged handler, call player.Play().
            return from trackAddress in PlayerService.GetTrackAddressAsync(this.NowPlaying.Set.Track).ObserveOn(Scheduler.CurrentThread).Do(
                    trackUrl =>
                    {
                        var coverUrl = this.NowPlaying.Cover.ThumbnailUrl;
                        var playControls = !this.NowPlaying.Set.IsLastTrack && this.NowPlaying.Set.SkipAllowed
                                               ? EnabledPlayerControls.Pause | EnabledPlayerControls.SkipNext
                                                 | EnabledPlayerControls.FastForward
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
                    })
                   select ObservableEx.Unit;
        }

        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">
        /// The BackgroundAudioPlayer
        /// </param>
        private IObservable<PortableUnit> SkipToNextTrackAsync(BackgroundAudioPlayer player)
        {
            Debug.WriteLine("Player: SkipToNextTrackAsync");

            if (!this.NowPlaying.Set.SkipAllowed)
            {
                return Observable.Empty<PortableUnit>();
            }

            var nextTrack = from nextResponse in this.NowPlaying.SkipToNextTrackAsync(player).Do(
                    response =>
                    {
                        this.NowPlaying.Set = response.Set;
                    })
                   from _ in this.NowPlaying.SaveNowPlayingAsync().ObserveOn(Scheduler.CurrentThread)
                   from play in this.PlayTrackAsync(player)
                   select play;

            return nextTrack.Catch<PortableUnit, WebException>(ex =>
                {
                    // TODO: Do more testing on this...
                    ////if (player.Track != null)
                    ////{
                    ////    player.Track.BeginEdit();
                    ////    player.Track.PlayerControls = EnabledPlayerControls.Pause;
                    ////    player.Track.EndEdit();
                    ////}

                    return Observable.Empty<PortableUnit>();
                }).OnErrorResumeNext(Observable.Empty<PortableUnit>());
        }

        /// <summary>
        /// </summary>
        /// <param name="player">
        /// </param>
        /// <returns>
        /// </returns>
        private IObservable<PortableUnit> StopPlayingAsync(BackgroundAudioPlayer player)
        {
            Debug.WriteLine("Player: StopPlayingAsync");
            return this.NowPlaying.StopAsync(player)
                .Catch<PortableUnit, ServiceException>(ex => ObservableEx.SingleUnit())
                .Catch<PortableUnit, WebException>(ex => ObservableEx.SingleUnit())
                .ObserveOn(Scheduler.CurrentThread)
                .Do(_ => this.StopPlayingMix(player), ex => this.StopPlayingMix(player));
        }

        /// <summary>
        /// </summary>
        /// <param name="player">
        /// </param>
        private void StopPlayingMix(BackgroundAudioPlayer player)
        {
            try
            {
                if (player.PlayerState != PlayState.Unknown && 
                    player.PlayerState != PlayState.Stopped && 
                    player.PlayerState != PlayState.Error)
                {
                        player.Stop();
                }
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                try
                {
                    player.Track = null;
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        #endregion
    }
}