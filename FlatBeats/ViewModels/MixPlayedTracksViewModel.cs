// --------------------------------------------------------------------------------------------------------------------
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
    using System.Linq;
    using System.Text;

    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class MixPlayedTracksViewModel : PanelViewModel
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
                IconUri = "/icons/appbar.transport.play.rest.png",
                Text = StringResources.Command_PlayMix
            };
            this.NextTrackCommand = new CommandLink()
            {
                Command = new DelegateCommand(this.SkipNext, this.CanSkipNext),
                IconUri = "/icons/appbar.transport.ff.rest.png",
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

        /// <summary>
        /// </summary>
        public ObservableCollection<TrackViewModel> Tracks { get; private set; }

        private bool CanPlay()
        {
            return this.currentMix != null;
        }

        private bool CanSkipNext()
        {
            return this.NowPlaying != null && this.currentMix != null && !this.NowPlaying.Set.IsLastTrack
                   && this.NowPlaying.Set.SkipAllowed;
        }

        /// <summary>
        /// </summary>
        public void Play()
        {
            if (this.NowPlaying != null)
            {
                if (this.Player.PlayerState == PlayState.Playing)
                {
                    this.Player.Pause();
                }
                else
                {
                    this.Player.Play();
                }

                return;
            }

            this.ShowProgress();
            var playSequence = from playResponse in this.currentMix.StartPlayingAsync().ObserveOnDispatcher().Do(
                m =>
                {
                    this.NowPlaying = m;
                    this.NowPlaying.SaveNowPlaying();
                    this.Player.Play();
                    this.UpdateIsNowPlaying();
                })
                select true;

            playSequence.Subscribe(this.isPlayingChanges.OnNext, this.HandleError, this.HideProgress);
        }

        private void ShowProgress()
        {
            // TODO: Implement
        }

        private void HideProgress()
        {
            // TODO: Implement
        }

        private void SkipNext()
        {
            this.Player.SkipNext();
        }

        private readonly Subject<bool> isPlayingChanges = new Subject<bool>();

        private bool isDataLoaded;

        public IObservable<bool> IsPlayingChanges 
        { 
            get
            {
                return this.isPlayingChanges;
            } 
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        protected BackgroundAudioPlayer Player { get; set; }

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

            if (this.isDataLoaded)
            {
                this.UpdatePlayerState();
                return Observable.Return(new Unit());
            }

            this.isDataLoaded = true;

            this.InitializeBackgroundAudioPlayer();

            this.LoadNowPlaying();
            this.UpdatePlayerState();

            if (this.PlayOnLoad)
            {
                this.PlayOnLoad = false;
                this.currentMix.StartPlayingAsync();
            }

            return RefreshPlayedTracksAsync(currentMix);
        }

        private void InitializeBackgroundAudioPlayer()
        {
            this.Player = BackgroundAudioPlayer.Instance;
            this.AddToLifetime(Observable.FromEvent<EventArgs>(this.Player, "PlayStateChanged").Subscribe(_ => this.UpdatePlayerState()));
        }

        private void LoadNowPlaying()
        {
            if (this.IsPlayingTrackForThisMix)
            {
                this.NowPlaying = PlayerService.LoadNowPlaying();
            }
            else
            {
                this.NowPlaying = null;
            }

            this.UpdateIsNowPlaying();
        }

        private IObservable<Unit> RefreshPlayedTracksAsync(MixContract loadMix)
        {
            this.Tracks.Clear();
            var tracks = from response in loadMix.PlayedTracksAsync()
                         where response != null && response.Tracks != null
                         from track in response.Tracks.ToObservable()
                         select new TrackViewModel(track);
            return tracks.Do(
                t => this.Tracks.Add(t), 
                this.UpdateMessage).FinallySelect(() => new Unit()).Catch<Unit, Exception>(
                    ex =>
                        {
                            this.HandleError(ex);
                            return Observable.Return(new Unit());
                        });
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
            if (this.refreshSubscription == null)
            {
                this.refreshSubscription = Observable.Interval(TimeSpan.FromSeconds(1), Scheduler.Dispatcher).Subscribe(_ => this.UpdatePlayingProgress());
                this.AddToLifetime(this.refreshSubscription);
            }
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
            this.ShowNowPlaying = this.IsPlayingTrackForThisMix;
            this.Title = this.ShowNowPlaying ? StringResources.Title_Playing : StringResources.Title_PlayedTracks;

            if (this.ShowNowPlaying)
            {
                if (this.Player.PlayerState == PlayState.Playing)
                {
                    this.PlayPauseCommand.IconUri = "/icons/appbar.transport.pause.rest.png";
                    this.PlayPauseCommand.Text = StringResources.Command_PauseMix;
                    //this.playStates.OnNext(true);
                }
                else
                {
                    this.PlayPauseCommand.IconUri = "/icons/appbar.transport.play.rest.png";
                    this.PlayPauseCommand.Text = StringResources.Command_PlayMix;
                    //this.playStates.OnNext(false);
                }
            }

            this.PlayPauseCommand.RaiseCanExecuteChanged();
            this.NextTrackCommand.RaiseCanExecuteChanged();
            this.UpdateMessage();
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
            switch (this.Player.PlayerState)
            {
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    this.StopRefreshTimer();
                    this.LoadNowPlaying();
                    break;
                case PlayState.Paused:
                    this.StopRefreshTimer();
                    this.ProgressStatusText = "Paused";
                    this.UpdateIsNowPlaying();
                    break;
                case PlayState.Playing:
                    this.LoadNowPlaying();
                    this.StartRefreshTimer();
                    this.EnsureCurrentTrackMatchesPlayingTrack();
                    break;
                case PlayState.BufferingStarted:
                    this.UpdateBufferingProgress();
                    this.EnsureCurrentTrackMatchesPlayingTrack();
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

                if (this.Player.Track == null)
                {
                    return false;
                }

                return this.Player.Track.Tag.StartsWith(this.currentMix.Id + "|");
            }
        }

        private bool IsPlayingTrackTheCurrentTrack
        {
            get
            {
                if (this.Player == null)
                {
                    return false;
                }
                
                if (this.Player.Track == null)
                {
                    return false;
                }

                if (this.CurrentTrack == null)
                {
                    return false;
                }

                return this.Player.Track.Tag.EndsWith("|" + this.CurrentTrack.Id);
            }
        }

        /// <summary>
        /// </summary>
        private void UpdatePlayingProgress()
        {
            if (!this.IsPlayingTrackTheCurrentTrack)
            {
                return;
            }

            this.Progress = this.Player.Position.TotalSeconds / Math.Max(1, this.Player.Track.Duration.TotalSeconds) * 100D;
            var sb = new StringBuilder();
            sb.AppendFormat("{0:00}:{1:00}", (int)this.Player.Position.TotalMinutes, this.Player.Position.Seconds);
            sb.Append(" / ");
            sb.AppendFormat("{0:00}:{1:00}", (int)this.Player.Track.Duration.TotalMinutes, this.Player.Track.Duration.Seconds);
            this.ProgressStatusText = sb.ToString();
            this.IsProgressIndeterminate = false;
        }

        #endregion
    }
}