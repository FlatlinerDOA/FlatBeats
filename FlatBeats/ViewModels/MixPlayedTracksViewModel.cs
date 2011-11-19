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
    using System.Text;

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
        private IDisposable playStateSubscription = Disposable.Empty;

        /// <summary>
        /// </summary>
        private double progress;

        /// <summary>
        /// </summary>
        private string progressStatusText;

        /// <summary>
        /// </summary>
        private IDisposable refreshSubscription = Disposable.Empty;

        /// <summary>
        /// </summary>
        private bool showNowPlaying;

        private string currentMixId;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the MixPlayedTracksViewModel class.
        /// </summary>
        public MixPlayedTracksViewModel()
        {
            this.Tracks = new ObservableCollection<TrackViewModel>();
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
                this.UpdateIsNowPlaying();
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

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        protected BackgroundAudioPlayer Player { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="mixData">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<Unit> LoadAsync(MixContract mixData)
        {
            this.currentMixId = mixData.Id;
            this.Player = BackgroundAudioPlayer.Instance;
            this.UpdateIsNowPlaying();
            this.playStateSubscription =
                Observable.FromEvent<EventArgs>(this.Player, "PlayStateChanged").Subscribe(
                    _ => this.UpdatePlayerState());
            this.UpdatePlayerState();

            return RefreshPlayedTracks(mixData);
        }

        private IObservable<Unit> RefreshPlayedTracks(MixContract mixData)
        {
            this.Tracks.Clear();
            var tracks = from response in mixData.PlayedTracks()
                         where response.Tracks != null
                         from track in response.Tracks.ToObservable()
                         select new TrackViewModel(track);
            return tracks.ObserveOnDispatcher().Do(
                t => this.Tracks.Add(t), 
                this.UpdateMessage).FinallySelect(() => new Unit()).Catch<Unit, Exception>(
                    ex =>
                        {
                            this.ShowError(ex);
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
                this.Message = null;
            }
        }

        /// <summary>
        /// </summary>
        public void Unload()
        {
            this.refreshSubscription.Dispose();
            this.playStateSubscription.Dispose();
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        private void StartBufferingRefreshTimer()
        {
            this.UpdatBufferingProgress();
            this.refreshSubscription.Dispose();
            this.refreshSubscription =
                Observable.Interval(TimeSpan.FromMilliseconds(300), Scheduler.Dispatcher).Subscribe(
                    _ => this.UpdatBufferingProgress());
        }

        /// <summary>
        /// </summary>
        private void StartPlayRefreshTimer()
        {
            this.NowPlaying = PlayerService.LoadNowPlaying();
            this.Message = null;
            this.UpdatePlayingProgress();
            this.refreshSubscription.Dispose();
            this.refreshSubscription =
                Observable.Interval(TimeSpan.FromSeconds(1), Scheduler.Dispatcher).Subscribe(
                    _ => this.UpdatePlayingProgress());
        }

        /// <summary>
        /// </summary>
        private void UpdatBufferingProgress()
        {
            this.Progress = this.Player.BufferingProgress * 100D;
            this.ProgressStatusText = "Buffering ({0}%)";
            this.IsProgressIndeterminate = false;
        }

        /// <summary>
        /// </summary>
        private void UpdateIsNowPlaying()
        {
            this.ShowNowPlaying = this.NowPlaying != null && this.NowPlaying.MixId == this.currentMixId;
            this.Title = this.ShowNowPlaying ? StringResources.Title_Playing : StringResources.Title_PlayedTracks;
            if (this.NowPlaying != null && this.NowPlaying.Set != null && this.NowPlaying.Set.Track != null)
            {
                this.CurrentTrack = new TrackViewModel(this.NowPlaying.Set.Track);
            }
            else
            {
                this.CurrentTrack = null;
                this.refreshSubscription.Dispose();
            }

            this.UpdateMessage();
        }

        /// <summary>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        private void UpdatePlayerState()
        {
            if (this.NowPlaying == null)
            {
                return;
            }

            switch (this.Player.PlayerState)
            {
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    this.refreshSubscription.Dispose();
                    this.ProgressStatusText = "Stopped";
                    this.Progress = 0;
                    this.IsProgressIndeterminate = true;
                    break;
                case PlayState.Paused:
                    this.refreshSubscription.Dispose();
                    this.ProgressStatusText = "Paused";
                    break;
                case PlayState.Playing:
                    this.StartPlayRefreshTimer();
                    break;
                case PlayState.BufferingStarted:
                    this.StartBufferingRefreshTimer();
                    break;
                case PlayState.BufferingStopped:
                    this.refreshSubscription.Dispose();
                    break;
                case PlayState.TrackReady:
                    break;
                case PlayState.TrackEnded:
                    this.Tracks.Insert(0, this.CurrentTrack);
                    this.refreshSubscription.Dispose();
                    break;
                case PlayState.Rewinding:
                    this.refreshSubscription.Dispose();
                    break;
                case PlayState.FastForwarding:
                    this.refreshSubscription.Dispose();
                    break;
                case PlayState.Shutdown:
                    this.refreshSubscription.Dispose();
                    break;
                case PlayState.Error:
                    this.refreshSubscription.Dispose();
                    this.ProgressStatusText = "Error";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        private void UpdatePlayingProgress()
        {
            if (this.Player.Track == null)
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