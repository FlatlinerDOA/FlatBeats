﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MixPlayedTracksViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Flatliner.Phone;
    using Flatliner.Phone.Core;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Net.NetworkInformation;
    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public sealed class MixPlayedTracksViewModel : PanelViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private TrackViewModel currentTrack;

        /// <summary>
        /// </summary>
        private bool isProgressIndeterminate;

        /// <summary>
        /// </summary>
        private PlayingMixContract nowPlaying;

        /// <summary>
        /// </summary>
        private double progress;

        /// <summary>
        /// </summary>
        private string progressStatusText;

        /// <summary>
        /// </summary>
        private IDisposable refreshSubscription;

        /// <summary>
        /// </summary>
        private bool showNowPlaying;

        private MixContract currentMix;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the MixPlayedTracksViewModel class.
        /// </summary>
        public MixPlayedTracksViewModel()
        {
            this.Tracks = new ObservableCollection<TrackViewModel>();
            this.Title = StringResources.Title_PlayedTracks;
            this.PlayPauseCommand = new CommandLink()
            {
                Command = new DelegateCommand(this.Play, this.CanPlay),
                IconUrl = new Uri("/icons/appbar.transport.play.rest.png", UriKind.Relative),
                Text = StringResources.Command_PlayMix
            };
            this.NextTrackCommand = new CommandLink()
            {
                Command = new DelegateCommand(this.SkipNext, this.CanSkipNext),
                IconUrl = new Uri("/icons/appbar.transport.ff.rest.png", UriKind.Relative),
                Text = StringResources.Command_NextTrack,
                HideWhenInactive = true
            };

            this.UpdateMessage();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public TrackViewModel CurrentTrack
        {
            get
            {
                return this.currentTrack;
            }

            set
            {
                if (this.currentTrack == value)
                {
                    return;
                }

                this.currentTrack = value;
                this.OnPropertyChanged("CurrentTrack");
            }
        }

        /// <summary>
        /// </summary>
        public bool IsProgressIndeterminate
        {
            get
            {
                return this.isProgressIndeterminate;
            }

            set
            {
                if (this.isProgressIndeterminate == value)
                {
                    return;
                }

                this.isProgressIndeterminate = value;
                this.OnPropertyChanged("IsProgressIndeterminate");
            }
        }

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
                if (this.nowPlaying == value)
                {
                    return;
                }

                this.nowPlaying = value;
                this.OnPropertyChanged("NowPlaying");
            }
        }

        /// <summary>
        /// </summary>
        public double Progress
        {
            get
            {
                return this.progress;
            }

            set
            {
                if (this.progress == value)
                {
                    return;
                }

                this.progress = value;
                this.OnPropertyChanged("Progress");
            }
        }

        /// <summary>
        /// </summary>
        public string ProgressStatusText
        {
            get
            {
                return this.progressStatusText;
            }

            set
            {
                if (this.progressStatusText == value)
                {
                    return;
                }

                this.progressStatusText = value;
                this.OnPropertyChanged("ProgressStatusText");
            }
        }

        /// <summary>
        /// </summary>
        public bool ShowNowPlaying
        {
            get
            {
                return this.showNowPlaying;
            }

            set
            {
                if (this.showNowPlaying == value)
                {
                    return;
                }

                this.showNowPlaying = value;
                this.OnPropertyChanged("ShowNowPlaying");
            }
        }

        public bool PlayOverWifiOnly { get; set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<TrackViewModel> Tracks { get; private set; }

        private bool CanPlay()
        {
            if (this.PlayOverWifiOnly && NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
            {
                this.PlayPauseCommand.Text = "no wifi";
                return false;
            }

            return this.currentMix != null;
        }

        private bool CanSkipNext()
        {
            if (this.NowPlaying == null || this.NowPlaying.Set == null)
            {
                return false;
            }

            return this.IsPlayingTrackForThisMix && !this.NowPlaying.Set.IsLastTrack
                   && this.NowPlaying.Set.SkipAllowed;
        }

        /// <summary>
        /// If the current mix is not playing, stops whatever is playing starts this mix, otherwise toggles between play and pause.
        /// </summary>
        public void Play()
        {
            if (this.IsPlayingTrackForThisMix)
            {
                if (this.Player.PlayerState == PlayState.Playing)
                {
                    this.ShowProgress("Pausing...");
                    this.Player.Pause();
                }
                else
                {
                    this.ShowProgress("Playing...");
                    this.Player.Play();
                }

                return;
            }

            if (this.Player.IsPlayingATrack())
            {
                this.ShowProgress("Stopping previous mix...");

                // Get what is currently playing and report it
                this.AddToLifetime((from playing in PlayerService.LoadNowPlayingAsync()
                                   from _ in playing.AddToMixTrackHistoryAsync(this.Player.Track.Duration)
                                   select _).Finally(this.StartPlayingMixFromBeginning).Subscribe(
                                       _ => { }, this.HandleError));
                return;
            }

            this.StartPlayingMixFromBeginning();
        }

        /// <summary>
        /// Starts playing this mix from the beginning.
        /// </summary>
        private void StartPlayingMixFromBeginning()
        {
            this.ShowProgress("Playing...");
            var playSequence = from playResponse in this.currentMix.StartPlayingAsync().ObserveOnDispatcher().Do(
                m =>
                    {
                        this.NowPlaying = m;
                    })
                    from _ in this.NowPlaying.SaveNowPlayingAsync().ObserveOnDispatcher()
                    .Do(_ =>
                    {
                        try
                        {
                            this.Player.Play();
                        } 
                        catch (InvalidOperationException)
                        {
                            this.HideProgress();
                        }

                        this.UpdateIsNowPlaying();
                    })
                    select true;

            playSequence.Subscribe(this.IsPlayingChanges.OnNext, this.HandleError);
        }

        private void SkipNext()
        {
            this.ShowProgress("Skipping... (you can only do this twice)");
            this.Player.SkipNext();
        }

        private Subject<bool> isPlayingChanges;

        private bool isDataLoaded;

        public Subject<bool> IsPlayingChanges 
        { 
            get
            {
                return this.isPlayingChanges = (this.isPlayingChanges ?? new Subject<bool>());
            } 
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        private BackgroundAudioPlayer Player { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="loadMix">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<Unit> LoadAsync(MixContract loadMix)
        {
            this.currentMix = loadMix;
            this.InitializeBackgroundAudioPlayer();

            if (this.isDataLoaded)
            {
                this.UpdatePlayerState();
                return this.RefreshPlayedTracksAsync(this.currentMix);
            }

            this.isDataLoaded = true;
            this.LoadNowPlaying();
            this.UpdatePlayerState();

            if (this.PlayOnLoad)
            {
                this.PlayOnLoad = false;
                if (!this.IsPlayingTrackForThisMix)
                {
                    this.StartPlayingMixFromBeginning();
                }
            }

            return this.RefreshPlayedTracksAsync(this.currentMix);
        }

        private void InitializeBackgroundAudioPlayer()
        {
            try
            {
                this.Player = BackgroundAudioPlayer.Instance;
                this.AddToLifetime(
                    Observable.FromEvent<EventArgs>(this.Player, "PlayStateChanged").Subscribe(
                        _ => this.UpdatePlayerState(), this.HandleError));
            } 
            catch (ArgumentException)
            {
                this.Player = null;
                this.ShowErrorMessage(new ErrorMessage(
                    StringResources.Error_BackgroundAudioPlayer_Title, 
                    StringResources.Error_BackgroundAudioPlayer_Message));
            }
        }

        private void LoadNowPlaying()
        {
            if (this.IsPlayingTrackForThisMix)
            {
                // TODO: HACK: Make async
                this.NowPlaying = PlayerService.LoadNowPlayingAsync().FirstOrDefault();
            }
            else
            {
                this.NowPlaying = null;
            }

            this.UpdateIsNowPlaying();
        }

        public override void Unload()
        {
            if (this.isPlayingChanges != null)
            {
                this.isPlayingChanges.OnCompleted();
                this.isPlayingChanges = null;
            }

            base.Unload();
        }

        private IObservable<Unit> RefreshPlayedTracksAsync(MixContract loadMix)
        {
            this.Tracks.Clear();
            this.ShowProgress(StringResources.Progress_Loading);
            var tracks = from response in loadMix.PlayedTracksAsync()
                         where response != null && response.Tracks != null
                         from track in response.Tracks.ToObservable()
                         select new TrackViewModel(track);
            return tracks.ObserveOnDispatcher().Do(this.AddToTrackToList, this.UpdateMessage).FinallySelect(() => new Unit()).Finally(this.HideProgress);
        }

        private void AddToTrackToList(TrackViewModel track)
        {
            var existing = this.Tracks.FirstOrDefault(t => t.Id == track.Id);
            if (existing != null)
            {
                this.Tracks.Remove(existing);
            }

            this.Tracks.Add(track);
        }

        private void UpdateMessage()
        {
            if (this.CurrentTrack == null && this.Tracks.Count == 0)
            {
                this.Message = StringResources.Message_NoRecentlyPlayedTracks;
                this.ShowMessage = true;
            }
            else
            {
                this.RemoveCurrentTrackFromList();
                this.Message = null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        private void StopRefreshTimer()
        {
            if (this.refreshSubscription == null)
            {
                return;
            }

            this.refreshSubscription.Dispose();
            this.refreshSubscription = null;
        }

        /// <summary>
        /// </summary>
        private void StartRefreshTimer()
        {
            if (!this.ShowNowPlaying)
            {
                return;
            }

            this.Message = null;
            this.UpdatePlayingProgress();
            if (this.refreshSubscription != null)
            {
                this.refreshSubscription.Dispose();
            }

            this.refreshSubscription = Observable.Interval(TimeSpan.FromSeconds(1), Scheduler.Dispatcher).Subscribe(_ => this.UpdatePlayingProgress(), this.HandleError);
            this.AddToLifetime(this.refreshSubscription);
        }

        /// <summary>
        /// </summary>
        private void UpdateBufferingProgress()
        {
            this.Progress = this.Player.BufferingProgress * 100D;
            this.ProgressStatusText = "Buffering {0}%";
            this.IsProgressIndeterminate = false;
        }

        public CommandLink NextTrackCommand { get; private set; }

        public CommandLink PlayPauseCommand { get; private set; }

        public bool PlayOnLoad { get; set; }

        /// <summary>
        /// </summary>
        public void UpdateIsNowPlaying()
        {
            this.EnsureCurrentTrackMatchesPlayingTrack();
            this.ShowNowPlaying = this.IsPlayingTrackForThisMix && this.CurrentTrack != null;
            this.Title = this.ShowNowPlaying ? StringResources.Title_Playing : StringResources.Title_PlayedTracks;

            if (this.ShowNowPlaying)
            {
                this.UpdatePlayPauseCommand();
            }

            this.PlayPauseCommand.RaiseCanExecuteChanged();
            this.NextTrackCommand.RaiseCanExecuteChanged();
            this.UpdateMessage();
        }

        private void UpdatePlayPauseCommand()
        {
            if (this.Player.PlayerState == PlayState.Playing)
            {
                this.PlayPauseCommand.IconUrl = new Uri("/icons/appbar.transport.pause.rest.png", UriKind.Relative);
                this.PlayPauseCommand.Text = StringResources.Command_PauseMix;
            }
            else
            {
                this.PlayPauseCommand.IconUrl = new Uri("/icons/appbar.transport.play.rest.png", UriKind.Relative);
                this.PlayPauseCommand.Text = StringResources.Command_PlayMix;
            }
        }

        /// <summary>
        /// Checks the current track matches the playing track and if not
        /// will move the current track to the played list.
        /// </summary>
        private void EnsureCurrentTrackMatchesPlayingTrack()
        {
            if (!this.IsPlayingTrackForThisMix)
            {
                this.MoveCurrentTrackToList();
                return;
            }

            if (!this.IsPlayingTrackTheCurrentTrack)
            {
                this.MoveCurrentTrackToList();

                if (this.NowPlaying != null && this.NowPlaying.Set != null && this.NowPlaying.Set.Track != null)
                {
                    this.CurrentTrack = new TrackViewModel(this.NowPlaying.Set.Track);
                    this.RemoveCurrentTrackFromList();
                }
            }
        }

        private void MoveCurrentTrackToList()
        {
            if (this.CurrentTrack == null)
            {
                return;
            }

            if (!this.Tracks.Any(t => t.Id == this.CurrentTrack.Id))
            {
                this.Tracks.Insert(0, this.CurrentTrack);
            }

            this.CurrentTrack = null;
        }

        private void RemoveCurrentTrackFromList()
        {
            if (this.CurrentTrack == null)
            {
                return;
            }

            var existing = this.Tracks.FirstOrDefault(t => t.Id == this.CurrentTrack.Id);
            if (existing != null)
            {
                this.Tracks.Remove(existing);
            }
        }

        /// <summary>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        private void UpdatePlayerState()
        {
            this.HideProgress();

            PlayState playState = PlayState.Unknown;            
            try
            {
                if (this.Player != null)
                {
                    playState = this.Player.PlayerState;
                }
            } 
            catch (InvalidOperationException)
            {
                // Background audio resources no longer allocated
            }

            Debug.WriteLine("UpdatePlayerState to " + Enum.GetName(typeof(PlayState), playState));
            switch (playState)
            {
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    this.StopRefreshTimer();
                    this.NowPlaying = null;
                    this.UpdateIsNowPlaying();
                    break;
                case PlayState.Paused:
                    this.StopRefreshTimer();
                    this.ProgressStatusText = "Paused";
                    this.UpdateIsNowPlaying();
                    break;
                case PlayState.Playing:
                    this.LoadNowPlaying();
                    this.StartRefreshTimer();
                    this.UpdateIsNowPlaying();
                    break;
                case PlayState.BufferingStarted:
                    this.UpdateBufferingProgress();
                    this.UpdateIsNowPlaying();
                    break;
                case PlayState.BufferingStopped:
                    this.UpdateBufferingProgress();
                    break;
                case PlayState.TrackReady:
                    break;
                case PlayState.TrackEnded:
                    this.LoadNowPlaying();
                    break;
                case PlayState.Rewinding:
                    this.Progress = 0;
                    this.IsProgressIndeterminate = false;
                    this.ProgressStatusText = "Rewinding";
                    break;
                case PlayState.FastForwarding:
                    this.Progress = 0;
                    this.IsProgressIndeterminate = false;
                    this.ProgressStatusText = "Fast forwarding";
                    break;
                case PlayState.Shutdown:
                    this.StopRefreshTimer();
                    break;
                case PlayState.Error:
                    this.StopRefreshTimer();
                    this.ProgressStatusText = "Error";
                    this.IsProgressIndeterminate = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets a value indicating that the playing track is part of the current mix
        /// </summary>
        private bool IsPlayingTrackForThisMix
        {
            get
            {
                if (this.Player == null)
                {
                    return false;
                }

                try
                {
                    if (!this.Player.IsPlayingATrack() || this.Player.Track.Tag == null)
                    {
                        return false;
                    }

                    return this.Player.Track.Tag.StartsWith(this.currentMix.Id + "|");
                }
                catch (Exception)
                {
                    // Gotta catch em all!
                    return false;
                }
            }
        }

        private bool IsPlayingTrackTheCurrentTrack
        {
            get
            {
                if (this.CurrentTrack == null)
                {
                    return false;
                }

                if (this.Player == null)
                {
                    return false;
                }

                try
                {
                    if (!this.Player.IsPlayingATrack() || this.Player.Track.Tag == null)
                    {
                        return false;
                    }

                    return this.Player.Track.Tag.EndsWith("|" + this.CurrentTrack.Id);
                } 
                catch (Exception)
                {
                    // Gotta catch em all!
                    return false;
                }
            }
        }

        /// <summary>
        /// </summary>
        private void UpdatePlayingProgress()
        {
            try
            {
                if (!this.IsPlayingTrackTheCurrentTrack)
                {
                    return;
                }

                var track = this.Player.Track;
                if (track == null)
                {
                    return;
                }

                var duration = track.Duration;
                var position = this.Player.Position;

                this.Progress = position.TotalSeconds / Math.Max(1, duration.TotalSeconds) * 100D;
                var sb = new StringBuilder();
                sb.Append(position.ToFormattedString());
                sb.Append(" / ");
                sb.Append(duration.ToFormattedString());
                this.ProgressStatusText = sb.ToString();
                this.IsProgressIndeterminate = false;
            }
            catch (Exception)
            {
                // Ignore issues accessing the track.
            }
        }

        #endregion
    }
}