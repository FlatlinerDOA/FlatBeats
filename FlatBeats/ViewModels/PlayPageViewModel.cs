// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayPageViewModel.cs" company="">
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

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;

    /// <summary>
    /// </summary>
    public class PlayPageViewModel : ViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private TrackViewModel currentTrack;

        /// <summary>
        /// </summary>
        private bool isInProgress;

        /// <summary>
        /// </summary>
        private string message;

        /// <summary>
        /// </summary>
        private MixViewModel mix;

        /// <summary>
        /// </summary>
        private MixContract mixData;

        /// <summary>
        /// </summary>
        private bool showNowPlaying;

        /// <summary>
        /// </summary>
        private string title;

        private readonly Subject<bool> playStates = new Subject<bool>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the PlayPageViewModel class.
        /// </summary>
        public PlayPageViewModel()
        {
            this.PlayedTracks = new ObservableCollection<TrackViewModel>();
            this.Reviews = new ObservableCollection<ReviewViewModel>();
        }

        private void PlayStateChanged(object sender, EventArgs e)
        {
            this.UpdatePlayState();
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
        public bool IsInProgress
        {
            get
            {
                return this.isInProgress;
            }

            set
            {
                if (this.isInProgress == value)
                {
                    return;
                }

                this.isInProgress = value;
                this.OnPropertyChanged("IsInProgress");
            }
        }

        /// <summary>
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (this.message == value)
                {
                    return;
                }

                this.message = value;
                this.OnPropertyChanged("Message");
            }
        }

        /// <summary>
        /// </summary>
        public MixViewModel Mix
        {
            get
            {
                return this.mix;
            }

            set
            {
                if (this.mix == value)
                {
                    return;
                }

                this.mix = value;
                this.OnPropertyChanged("Mix");
            }
        }

        /// <summary>
        /// </summary>
        public string MixId { get; set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<TrackViewModel> PlayedTracks { get; private set; }

        /// <summary>
        /// </summary>
        public BackgroundAudioPlayer Player { get; set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<ReviewViewModel> Reviews { get; private set; }

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

        private bool hasPlayedTracks;

        public bool HasPlayedTracks
        {
            get
            {
                return this.hasPlayedTracks;
            }
            set
            {
                if (this.hasPlayedTracks == value)
                {
                    return;
                }

                this.hasPlayedTracks = value;
                this.OnPropertyChanged("HasPlayedTracks");
            }
        }

        /// <summary>
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (this.title == value)
                {
                    return;
                }

                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        protected bool IsDataLoaded { get; set; }

        /// <summary>
        /// </summary>
        protected PlayingMixContract NowPlaying { get; set; }

        public IObservable<bool> PlayStates 
        { 
            get
            {
                return this.playStates;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public void Load()
        {
            this.Player = BackgroundAudioPlayer.Instance;
            this.Player.PlayStateChanged += this.PlayStateChanged;

            if (this.IsDataLoaded)
            {
                this.UpdatePlayState();
                this.LoadPlayedTracks();
                return;
            }

            this.IsDataLoaded = true;
            this.IsInProgress = true;
            var downloadMix = Downloader.GetJson<MixResponseContract>(
                    new Uri(string.Format("http://8tracks.com/mixes/{0}.json", this.MixId), UriKind.RelativeOrAbsolute), 
                    "mix-" + this.MixId + ".json");

            downloadMix.ObserveOnDispatcher().Subscribe(
                mixes => this.LoadMix(mixes.Mix), this.ShowError, this.LoadComments);
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

            var playResponse = this.mixData.StartPlaying();
            playResponse.ObserveOnDispatcher().Subscribe(
                m =>
                    {
                        this.NowPlaying = m;
                        this.NowPlaying.Save();
                        this.Player.Play();
                    });
        }

        /// <summary>
        /// </summary>
        public void Share()
        {
            var task = new ShareLinkTask
                {
                    Title = "Share mix", 
                    Message = this.Mix.MixName, 
                    LinkUri = this.Mix.LinkUrl
                };
            task.Show();
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        private void HideProgress()
        {
            this.IsInProgress = false;
        }

        /// <summary>
        /// </summary>
        private void LoadComments()
        {
            var downloadComments =
                from response in
                    Downloader.GetJson<ReviewsResponseContract>(
                        new Uri(
                    string.Format("http://8tracks.com/mixes/{0}/reviews.json?per_page=20", this.MixId), 
                    UriKind.RelativeOrAbsolute))
                from review in response.Reviews.ToObservable()
                select new ReviewViewModel(review);
            downloadComments.ObserveOnDispatcher().Subscribe(
                r => this.Reviews.Add(r), this.ShowError, this.LoadPlayedTracks);
        }

        private void LoadPlayedTracks()
        {
            this.PlayedTracks.Clear();
            var tracks = from response in this.mixData.PlayedTracks()
                         where response.Tracks != null
                         from track in response.Tracks.ToObservable()
                             select new TrackViewModel(track);
            tracks.ObserveOnDispatcher().Subscribe(t => this.PlayedTracks.Add(t), this.ShowError, () =>
                {
                    this.HasPlayedTracks = this.PlayedTracks.Count != 0;   
                    this.HideProgress();
                });
        }

        /// <summary>
        /// </summary>
        /// <param name="loadMix">
        /// </param>
        private void LoadMix(MixContract loadMix)
        {
            this.Title = loadMix.Name;
            this.mixData = loadMix;
            this.Mix = new MixViewModel(loadMix);
            if (this.Player.Track != null && this.Player.Track.Tag.StartsWith(loadMix.Id + "|"))
            {
                this.NowPlaying = PlayerService.Load();
            }

            this.ShowNowPlaying = this.NowPlaying != null;
            this.UpdatePlayState();
        }

        private void UpdatePlayState()
        {
            if (this.NowPlaying != null)
            {
                if (this.Player.PlayerState == PlayState.Playing)
                {
                    this.playStates.OnNext(true);
                
                }
                else
                {
                    this.playStates.OnNext(false);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="obj">
        /// </param>
        private void ShowError(Exception obj)
        {
            this.IsInProgress = false;
            this.Message = obj.Message;
        }

        #endregion

        public void Email()
        {
            var task = new EmailComposeTask()
                {
                    Body = this.Mix.Description + " - " + this.Mix.LinkUrl.AbsoluteUri, 
                    Subject = this.Mix.MixName
                };
            task.Show();
        }
    }
}