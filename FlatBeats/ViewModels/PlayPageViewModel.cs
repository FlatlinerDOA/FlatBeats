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
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;


    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Tasks;

    /// <summary>
    /// </summary>
    public class PlayPageViewModel : PageViewModel, IApplicationBarViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private readonly Subject<bool> playStates = new Subject<bool>();

        /// <summary>
        /// </summary>
        private TrackViewModel currentTrack;

        /// <summary>
        /// </summary>
        private bool hasPlayedTracks;

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

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the PlayPageViewModel class.
        /// </summary>
        public PlayPageViewModel()
        {
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
            this.Played = new MixPlayedTracksViewModel();
            this.Reviews = new ObservableCollection<ReviewViewModel>();
            this.PlayPauseCommand = new CommandLink() { Command = new DelegateCommand(this.Play), IconUri = "/icons/appbar.transport.play.rest.png", Text = StringResources.Command_PlayMix };
            this.NextTrackCommand = new CommandLink() { Command = new DelegateCommand(this.SkipNext, this.CanSkipNext), IconUri = "/icons/appbar.transport.ff.rest.png", Text = StringResources.Command_NextTrack, HideWhenInactive = true };
            this.LikeUnlikeCommand = new CommandLink() { Command = new DelegateCommand(this.LikeUnlike), IconUri = "/icons/appbar.heart2.empty.rest.png", Text = StringResources.Command_LikeMix };
            this.ApplicationBarButtonCommands.Add(this.PlayPauseCommand);
            this.ApplicationBarButtonCommands.Add(this.NextTrackCommand);
            this.ApplicationBarButtonCommands.Add(this.LikeUnlikeCommand);

            this.ApplicationBarMenuCommands.Add(
                new CommandLink()
                    {
                        Command = new DelegateCommand(this.PinToStart), 
                        Text = StringResources.Command_PinToStart
                    });
            this.ApplicationBarMenuCommands.Add(new CommandLink()
                {
                    Text = StringResources.Command_ShareMix,
                    Command  = new DelegateCommand(this.Share)
                });

            this.ApplicationBarMenuCommands.Add(new CommandLink()
                {
                    Text = StringResources.Command_EmailMix,
                    Command = new DelegateCommand(this.Email)
                });
        }

        public CommandLink PinToStartCommand { get; private set; }

        private void PinToStart()
        {
            ShellTile.Create(new Uri("/PlayPage.xaml?mix=" + this.Mix.MixId, UriKind.Relative), new StandardTileData()
                {
                    Title = this.Mix.MixName,
                    BackContent = this.Mix.Description,
                    BackgroundImage = this.Mix.ThumbnailUrl,
                    BackTitle = this.Mix.MixName
                });
        }

        private bool CanSkipNext()
        {
            return this.NowPlaying != null;
        }

        private void LikeUnlike()
        {
            this.Mix.Liked = !this.Mix.Liked;
            this.UpdateLikedState();
            this.ShowProgress();
            ProfileService.SetMixLiked(this.MixId, this.Mix.Liked).ObserveOnDispatcher().Subscribe(
                _ => { }, 
                this.ShowError,
                this.HideProgress);
        }

        private void SkipNext()
        {
            this.Player.SkipNext();
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
        public IObservable<bool> PlayStates
        {
            get
            {
                return this.playStates;
            }
        }


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

        public MixPlayedTracksViewModel Played { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        protected bool IsDataLoaded { get; set; }

        /// <summary>
        /// </summary>
        protected PlayingMixContract NowPlaying { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public void Email()
        {
            var task = new EmailComposeTask
                {
                   Subject = StringResources.EmailBody_ShareMix,
                   Body = string.Format(
                       StringResources.EmailBody_ShareMix,
                       this.Mix.MixName,
                       this.Mix.Description, 
                       this.Mix.LinkUrl.AbsoluteUri)
                };
            task.Show();
        }

        /// <summary>
        /// </summary>
        public void Load()
        {
            this.Player = BackgroundAudioPlayer.Instance;
            this.Player.PlayStateChanged += this.PlayStateChanged;

            if (this.IsDataLoaded)
            {
                this.UpdatePlayState();
                this.Played.LoadAsync(this.mixData).Subscribe(_ => { }, this.HideProgress);
                return;
            }

            this.IsDataLoaded = true;
            this.ShowProgress();
            var downloadMix =
                from response in
                    Downloader.GetJson<MixResponseContract>(
                        new Uri(
                    string.Format("http://8tracks.com/mixes/{0}.json", this.MixId), UriKind.RelativeOrAbsolute))
                select response.Mix;

            downloadMix.ObserveOnDispatcher().Subscribe(this.LoadMix, this.ShowError, this.LoadComments);
        }

        public CommandLink PlayPauseCommand { get; private set; }

        public CommandLink NextTrackCommand { get; private set; }

        public CommandLink LikeUnlikeCommand { get; private set; }


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
            var playSequence = from playResponse in this.mixData.StartPlaying().ObserveOnDispatcher().Do(
                                m =>
                                {
                                    this.NowPlaying = m;
                                    this.NowPlaying.SaveNowPlaying();
                                    this.Player.Play();
                                })
                               from updateTile in
                                   PlayerService.SetNowPlayingTile(
                                       this.mixData,
                                       StringResources.Title_ApplicationName,
                                       StringResources.Title_NowPlaying)
                               select new Unit();

            playSequence.Subscribe(
                        _ => { }, 
                        this.ShowError, 
                        this.HideProgress);
        }

        /// <summary>
        /// </summary>
        public void Share()
        {
            var task = new ShareLinkTask
                {
                    Title = StringResources.Title_ShareMix, 
                    Message = this.Mix.MixName, 
                    LinkUri = this.Mix.LinkUrl
                };
            task.Show();
        }

        #endregion

        #region Methods

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
                r => this.Reviews.Add(r), 
                this.ShowError, 
                () => this.Played.LoadAsync(this.mixData).ObserveOnDispatcher().Subscribe(_ => { }, this.HideProgress));
        }

        /// <summary>
        /// </summary>
        /// <param name="loadMix">
        /// </param>
        private void LoadMix(MixContract loadMix)
        {
            this.Title = loadMix.Name;
            this.mixData = loadMix;
            this.CreatedByUserId = loadMix.User.Id;
            this.Mix = new MixViewModel(loadMix);
            if (this.PlayOnLoad)
            {
                this.PlayOnLoad = false;
                this.mixData.StartPlaying();
            }

            if (this.Player.Track != null && this.Player.Track.Tag.StartsWith(loadMix.Id + "|"))
            {
                this.NowPlaying = PlayerService.LoadNowPlaying();
            }

            this.UpdateLikedState();

            this.ShowNowPlaying = this.NowPlaying != null;
            this.UpdatePlayState();
        }

        private void UpdateLikedState()
        {
            if (this.Mix.Liked)
            {
                this.LikeUnlikeCommand.IconUri = "/icons/appbar.heart2.rest.png";
                this.LikeUnlikeCommand.Text = StringResources.Command_UnlikeMix;
            }
            else
            {
                this.LikeUnlikeCommand.IconUri = "/icons/appbar.heart2.empty.rest.png";
                this.LikeUnlikeCommand.Text = StringResources.Command_LikeMix;
            }

            ((DelegateCommand)this.LikeUnlikeCommand.Command).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void PlayStateChanged(object sender, EventArgs e)
        {
            this.UpdatePlayState();
        }

        /// <summary>
        /// </summary>
        private void UpdatePlayState()
        {
            if (this.NowPlaying != null)
            {
                if (this.Player.PlayerState == PlayState.Playing)
                {
                    this.PlayPauseCommand.IconUri = "/icons/appbar.transport.pause.rest.png";
                    this.PlayPauseCommand.Text = StringResources.Command_PauseMix;
                    this.playStates.OnNext(true);
                }
                else
                {
                    this.PlayPauseCommand.IconUri = "/icons/appbar.transport.play.rest.png";
                    this.PlayPauseCommand.Text = StringResources.Command_PlayMix;
                    this.playStates.OnNext(false);
                }

                ((DelegateCommand)this.PlayPauseCommand.Command).RaiseCanExecuteChanged();
                ((DelegateCommand)this.NextTrackCommand.Command).RaiseCanExecuteChanged();
            }
        }

        #endregion

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }

        public bool PlayOnLoad { get; set; }

        public string CreatedByUserId { get; set; }
    }
}