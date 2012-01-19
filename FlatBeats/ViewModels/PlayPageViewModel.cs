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

    using Coding4Fun.Phone.Controls;

    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Flatliner.Phone;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;

    /// <summary>
    /// </summary>
    public class PlayPageViewModel : PageViewModel, IApplicationBarViewModel
    {
        #region Constants and Fields

        private int currentPanelIndex;

        /// <summary>
        /// </summary>
        private MixViewModel mix;

        /// <summary>
        /// </summary>
        private MixContract mixData;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the PlayPageViewModel class.
        /// </summary>
        public PlayPageViewModel()
        {
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
            this.PlayedPanel = new MixPlayedTracksViewModel();
            this.ReviewsPanel = new ReviewsPanelViewModel();
            this.ReviewMixCommand = new CommandLink()
                {
                    Command = new DelegateCommand(this.AddReview, this.CanAddReview),
                    Text = StringResources.Command_ReviewMix
                };
            this.LikeUnlikeCommand = new CommandLink()
                {
                    Command = new DelegateCommand(this.LikeUnlike, this.CanLikeUnlike),
                    IconUrl = new Uri("/icons/appbar.heart2.empty.rest.png", UriKind.Relative),
                    Text = StringResources.Command_LikeMix
                };
            this.PinToStartCommand = new CommandLink()
                {
                    Command = new DelegateCommand(this.PinToStart), 
                    Text = StringResources.Command_PinToStart
                };
            this.ApplicationBarButtonCommands.Add(this.PlayedPanel.PlayPauseCommand);
            this.ApplicationBarButtonCommands.Add(this.PlayedPanel.NextTrackCommand);
            this.ApplicationBarButtonCommands.Add(this.LikeUnlikeCommand);
            this.ApplicationBarMenuCommands.Add(this.ReviewMixCommand);
            this.ApplicationBarMenuCommands.Add(this.PinToStartCommand);
            this.ApplicationBarMenuCommands.Add(
                new CommandLink() { Text = StringResources.Command_ShareMix, Command = new DelegateCommand(this.Share) });

            this.ApplicationBarMenuCommands.Add(
                new CommandLink() { Text = StringResources.Command_EmailMix, Command = new DelegateCommand(this.Email) });
            this.Title = StringResources.Title_Mix;
        }

        #endregion

        #region Public Properties

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }

        public string CreatedByUserId { get; set; }

        public int CurrentPanelIndex
        {
            get
            {
                return this.currentPanelIndex;
            }
            set
            {
                if (this.currentPanelIndex == value)
                {
                    return;
                }

                this.currentPanelIndex = value;
                this.OnPropertyChanged("CurrentPanelIndex");
            }
        }

        public CommandLink LikeUnlikeCommand { get; private set; }

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

        public CommandLink PinToStartCommand { get; private set; }

        public MixPlayedTracksViewModel PlayedPanel { get; private set; }

        public CommandLink ReviewMixCommand { get; set; }

        public ReviewsPanelViewModel ReviewsPanel { get; private set; }

        #endregion


        #region Public Methods

        /// <summary>
        /// </summary>
        public void Email()
        {
            var task = new EmailComposeTask
                {
                    Subject = StringResources.EmailSubject_ShareMix,
                    Body =
                        string.Format(
                            StringResources.EmailBody_ShareMix,
                            this.Mix.MixName,
                            this.Mix.Description,
                            this.Mix.LinkUrl.AbsoluteUri)
                };
            task.Show();
        }

        /// <summary>
        /// </summary>
        public override void Load()
        {
            this.AddToLifetime(
                this.PlayedPanel.IsPlayingChanges.Where(playing => playing).Subscribe(
                    _ => 
                    { 
                        this.CurrentPanelIndex = 2; 
                    }));

            this.AddToLifetime(this.PlayedPanel.IsInProgressChanges.Subscribe(t => this.UpdateIsInProgress()));
            this.AddToLifetime(this.ReviewsPanel.IsInProgressChanges.Subscribe(t => this.UpdateIsInProgress()));

            if (this.IsDataLoaded)
            {
                this.UpdatePinnedState();
                this.ShowProgress();
                this.AddToLifetime(
                    this.PlayedPanel.LoadAsync(this.mixData).ObserveOnDispatcher().Subscribe(
                    _ => { }, this.HandleError, this.HideProgress));
                return;
            }

            this.ShowProgress();
            var loadProcess = from mix in this.LoadMixAsync(this.MixId).TakeLast(1)
                              from played in this.PlayedPanel.LoadAsync(mix)
                              select mix;
            this.AddToLifetime(
                loadProcess.ObserveOnDispatcher().Subscribe(
                    _ => this.UpdatePinnedState(), this.HandleError, this.LoadCompleted));

            this.AddToLifetime(this.ReviewsPanel.LoadAsync(this.MixId).Subscribe(_ => { }, this.HandleError));
            this.ReviewsPanel.LoadNextPage();
        }

        private void UpdateIsInProgress()
        {
            if (this.ReviewsPanel.IsInProgress || this.PlayedPanel.IsInProgress || this.Mix.IsLoading)
            {
                this.ShowProgress();
            }
            else
            {
                this.HideProgress();
            }
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

        public override void Unload()
        {
            this.PlayedPanel.Unload();
        }

        #endregion

        #region Methods

        private void AddReview()
        {
            var prompt = new InputPrompt();
            var completed =
                Observable.FromEvent<PopUpEventArgs<string, PopUpResult>>(
                    handler => prompt.Completed += handler, handler => prompt.Completed -= handler);

            var q = from response in completed.Take(1)
                    where response.EventArgs.PopUpResult == PopUpResult.Ok
                    from reviewAdded in ProfileService.AddMixReview(this.MixId, response.EventArgs.Result)
                    select reviewAdded;
            q.ObserveOnDispatcher().Subscribe(
                review => this.ReviewsPanel.Reviews.Insert(0, new ReviewViewModel(review.Review)),
                this.HandleError,
                this.HideProgress);
            prompt.Show();
        }

        private bool CanAddReview()
        {
            return Downloader.IsAuthenticated && this.mixData != null;
        }

        private bool CanLikeUnlike()
        {
            return Downloader.IsAuthenticated && this.mixData != null;
        }

        private void LikeUnlike()
        {
            this.Mix.Liked = !this.Mix.Liked;
            this.UpdateLikedState();
            this.ShowProgress();
            ProfileService.SetMixLiked(this.MixId, this.Mix.Liked).ObserveOnDispatcher().Subscribe(
                _ => { }, this.HandleError, this.HideProgress);
        }

        /// <summary>
        /// </summary>
        /// <param name = "loadMix">
        /// </param>
        private void LoadMix(MixContract loadMix)
        {
            this.mixData = loadMix;
            this.CreatedByUserId = loadMix.User.Id;
            this.Mix = new MixViewModel(loadMix);
            this.UpdateLikedState();

            this.UpdatePinnedState();
            this.ReviewMixCommand.RaiseCanExecuteChanged();
        }

        private IObservable<MixContract> LoadMixAsync(string id)
        {
            return MixesService.GetMixAsync(id).ObserveOnDispatcher().Do(this.LoadMix);
        }

        private void PinToStart()
        {
            if (MixesService.IsPinned(this.mixData))
            {
                MixesService.UnpinFromStart(this.mixData);
            }
            else
            {
                MixesService.PinToStart(this.mixData);
            }

            this.UpdatePinnedState();
        }

        private void UpdateLikedState()
        {
            if (this.Mix.Liked)
            {
                this.LikeUnlikeCommand.IconUrl = new Uri("/icons/appbar.heart2.rest.png", UriKind.Relative);
                this.LikeUnlikeCommand.Text = StringResources.Command_UnlikeMix;
            }
            else
            {
                this.LikeUnlikeCommand.IconUrl = new Uri("/icons/appbar.heart2.empty.rest.png", UriKind.Relative);
                this.LikeUnlikeCommand.Text = StringResources.Command_LikeMix;
            }

            this.LikeUnlikeCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// </summary>
        private void UpdatePinnedState()
        {
            if (MixesService.IsPinned(this.mixData))
            {
                this.PinToStartCommand.Text = StringResources.Command_UnpinStart;
            }
            else
            {
                this.PinToStartCommand.Text = StringResources.Command_PinToStart;
            }
        }

        #endregion
    }
}