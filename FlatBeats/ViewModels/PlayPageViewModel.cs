namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using FlatBeats.DataModel;
    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;

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
        private string mixName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the PlayPageViewModel class.
        /// </summary>
        public PlayPageViewModel()
        {
            this.Comments = new ObservableCollection<string>();
            this.Player = BackgroundAudioPlayer.Instance;
            this.Player.PlayStateChanged += this.PlayStateChanged;
        }

        #endregion

        #region Public Properties

        private bool isInProgress;

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
        public string MixId { get; set; }

        private string title;

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
        private string message;

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

        public ObservableCollection<string> Comments { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<TrackViewModel> PlayedTracks { get; private set; }

        /// <summary>
        /// </summary>
        public BackgroundAudioPlayer Player { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public void Load()
        {
            this.IsInProgress = true;
            var downloadMix = 
            Downloader.DownloadJson<SingleMixContract>(
                new Uri(string.Format("http://8tracks.com/mixes/{0}.json", this.MixId), UriKind.RelativeOrAbsolute), 
                "playingmix.json");
            
            downloadMix.ObserveOnDispatcher().Subscribe(mixes => this.LoadMix(mixes.Mix), this.ShowError, this.LoadComments);
        }

        private void LoadComments()
        {
            var downloadComments =
                from response in
                    Downloader.DownloadJson<ReviewsResponseContract>(
                        new Uri(
                    string.Format("http://8tracks.com/mixes/{0}/reviews.json?per_page=10", this.MixId),
                    UriKind.RelativeOrAbsolute))
                from review in response.Reviews.ToObservable()
                select review;
            downloadComments.ObserveOnDispatcher().Subscribe(
                r => this.Comments.Add(r.Body), this.ShowError, this.HideProgress);
        }

        private void HideProgress()
        {
            this.IsInProgress = false;
        }

        private void ShowError(Exception obj)
        {
            this.IsInProgress = false;
            this.Message = obj.Message;
        }

        /// <summary>
        /// </summary>
        public void LoadPlaying()
        {
            //// http://8tracks.com/sets/460486803/tracks_played.xml?mix_id=2000
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="loadMix">
        /// </param>
        private void LoadMix(MixContract loadMix)
        {
            this.Title = loadMix.Name;
            this.Mix = new MixViewModel(loadMix);
            
            ////this.Player.Track = new AudioTrack(this.CurrentTrack.AudioUrl, this.CurrentTrack.Title, this.MixName, "", new Uri());
        }

        private MixViewModel mix;

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
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void PlayStateChanged(object sender, EventArgs e)
        {

        }

        #endregion
    }

    /// <summary>
    /// </summary>
    public class TrackViewModel : ViewModel
    {
        #region Public Properties

        /// <summary>
        /// </summary>
        public Uri AudioUrl { get; private set; }

        /// <summary>
        /// </summary>
        public string Title { get; private set; }

        #endregion
    }
}