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
    using System.Windows.Input;

    using Coding4Fun.Phone.Controls;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.Services;

    using Flatliner.Phone;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;

    /// <summary>
    /// The play page view model.
    /// </summary>
    public sealed class PlayPageViewModel : PageViewModel, IApplicationBarViewModel
    {
        private readonly IAsyncDownloader downloader;

        private readonly ProfileService profileService;

        #region Constants and Fields

        /// <summary>
        /// The current panel index.
        /// </summary>
        private int currentPanelIndex;

        /// <summary>
        /// The mix.
        /// </summary>
        private MixViewModel mix;

        /// <summary>
        /// The mix data.
        /// </summary>
        private MixContract mixData;

        private bool censor;

        #endregion

        #region Constructors and Destructors
        /// <summary>
        /// Initializes a new instance of the PlayPageViewModel class.
        /// </summary>
        public PlayPageViewModel()
            : this(AsyncDownloader.Instance, ProfileService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PlayPageViewModel class.
        /// </summary>
        public PlayPageViewModel(IAsyncDownloader downloader, ProfileService profileService)
        {
            this.downloader = downloader;
            this.profileService = profileService;
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
            this.PlayedPanel = new MixPlayedTracksViewModel();
            this.ReviewsPanel = new ReviewsPanelViewModel();
            this.ReviewMixCommand = new CommandLink
                {
                    Command = new DelegateCommand(this.AddReview, this.CanAddReview), 
                    Text = StringResources.Command_ReviewMix
                };
            this.LikeUnlikeCommand = new CommandLink
                {
                    Command = new DelegateCommand(this.LikeUnlike, this.CanLikeUnlike), 
                    IconUrl = new Uri("/icons/appbar.heart2.empty.rest.png", UriKind.Relative), 
                    Text = StringResources.Command_LikeMix
                };
            this.PinToStartCommand = new CommandLink
                {
                   Command = new DelegateCommand(this.PinToStart), Text = StringResources.Command_PinToStart 
                };

            this.ShareCommand =  new CommandLink
                {
                    Text = StringResources.Command_ShareMix,
                    Command = new DelegateCommand(this.Share, () => this.Mix != null)
                };
            this.ApplicationBarButtonCommands.Add(this.PlayedPanel.PlayPauseCommand);
            this.ApplicationBarButtonCommands.Add(this.PlayedPanel.NextTrackCommand);
            this.ApplicationBarButtonCommands.Add(this.LikeUnlikeCommand);
            this.ApplicationBarMenuCommands.Add(this.ReviewMixCommand);
            this.ApplicationBarMenuCommands.Add(this.PinToStartCommand);
            this.ApplicationBarMenuCommands.Add(this.ShareCommand);

            this.EmailCommand = new CommandLink
                {
                    Text = StringResources.Command_EmailMix,
                    Command = new DelegateCommand(this.Email, () => this.Mix != null)
                };
            this.ApplicationBarMenuCommands.Add(this.EmailCommand);
               
            this.Title = StringResources.Title_Mix;
        }

        public CommandLink ShareCommand { get; set; }
        public CommandLink EmailCommand { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets ApplicationBarButtonCommands.
        /// </summary>
        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        /// <summary>
        /// Gets ApplicationBarMenuCommands.
        /// </summary>
        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }

        /// <summary>
        /// Gets or sets CreatedByUserId.
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the username of the user that created the mix
        /// </summary>
        public string CreatedByUserName
        {
            get
            {
                if (this.Mix != null)
                {
                    return this.Mix.CreatedBy;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets CurrentPanelIndex.
        /// </summary>
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

                this.LoadCurrentPanel();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsPromptOpen.
        /// </summary>
        public bool IsPromptOpen { get; set; }

        /// <summary>
        /// Gets LikeUnlikeCommand.
        /// </summary>
        public CommandLink LikeUnlikeCommand { get; private set; }

        /// <summary>
        /// Gets or sets Mix.
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
        /// Gets or sets MixId.
        /// </summary>
        public string MixId { get; set; }

        /// <summary>
        /// Gets PinToStartCommand.
        /// </summary>
        public CommandLink PinToStartCommand { get; private set; }

        /// <summary>
        /// Gets PlayedPanel.
        /// </summary>
        public MixPlayedTracksViewModel PlayedPanel { get; private set; }

        /// <summary>
        /// Gets or sets ReviewMixCommand.
        /// </summary>
        public CommandLink ReviewMixCommand { get; set; }

        /// <summary>
        /// Gets ReviewsPanel.
        /// </summary>
        public ReviewsPanelViewModel ReviewsPanel { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The email.
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
        /// The load.
        /// </summary>
        public override void Load()
        {
           

            this.AddToLifetime(
                this.PlayedPanel.IsPlayingChanges.Where(playing => playing).Subscribe(
                    _ => { this.CurrentPanelIndex = 2; }));

            this.AddToLifetime(this.PlayedPanel.IsInProgressChanges.Subscribe(t => this.UpdateIsInProgress()));
            this.AddToLifetime(this.ReviewsPanel.IsInProgressChanges.Subscribe(t => this.UpdateIsInProgress()));

            if (this.IsDataLoaded)
            {
                this.UpdatePinnedState();
                this.ShowProgress(StringResources.Progress_Loading);
                this.LoadCurrentPanel();
                this.AddToLifetime(
                    this.PlayedPanel.LoadAsync(this.mixData).ObserveOnDispatcher().Subscribe(
                        _ => { }, this.HandleError, this.HideProgress));
                return;
            }

            this.MixId = this.NavigationParameters["mix"];
            this.PlayedPanel.PlayOnLoad = this.NavigationParameters.ContainsKey("play")
                                          && this.NavigationParameters["play"] == "true";
            this.ShowProgress(StringResources.Progress_Loading);
            IObservable<Unit> login = from settings in this.profileService.GetSettingsAsync().Do(s =>
                                        { 
                                            this.censor = s.CensorshipEnabled;
                                            this.PlayedPanel.PlayOverWifiOnly = s.PlayOverWifiOnly;
                                        })
                                      from userToken in this.profileService.LoadUserTokenAsync().Select(_ => new Unit())
                                      select userToken;
            IObservable<Unit> loadMix = from mix in this.LoadMixAsync(this.MixId).TakeLast(1)
                                        from played in this.PlayedPanel.LoadAsync(mix)
                                        select new Unit();
            this.AddToLifetime(
                login.Concat(loadMix).ObserveOnDispatcher().Subscribe(
                    _ => this.UpdatePinnedState(), this.HandleError, this.LoadCompleted));
        }

        public string MixName
        {
            get
            {
                if (this.NavigationParameters.ContainsKey("title"))
                {
                    return this.NavigationParameters["title"];
                }

                if (this.Mix != null)
                {
                    return this.Mix.MixName;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// The share.
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

        /// <summary>
        /// The unload.
        /// </summary>
        public override void Unload()
        {
            this.PlayedPanel.Unload();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add review.
        /// </summary>
        private void AddReview()
        {
            var scope = new InputScope();
            scope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Chat });

            var prompt = new InputPrompt { InputScope = scope };
            IObservable<IEvent<PopUpEventArgs<string, PopUpResult>>> completed =
                Observable.FromEvent<PopUpEventArgs<string, PopUpResult>>(
                    handler => prompt.Completed += handler, handler => prompt.Completed -= handler);

            IObservable<ReviewResponseContract> q = from response in completed.Take(1).Do(
                _ =>
                    {
                        this.IsPromptOpen = false;
                        this.ShowProgress(StringResources.Progress_Updating);
                    })
                                                    where response.EventArgs.PopUpResult == PopUpResult.Ok
                                                    from reviewAdded in
                                                        this.profileService.AddMixReviewAsync(
                                                            this.MixId, response.EventArgs.Result)
                                                    select reviewAdded;
            q.ObserveOnDispatcher().Subscribe(
                review => this.ReviewsPanel.Items.Insert(0, new ReviewViewModel(review.Review, this.censor)), 
                this.HandleError, 
                this.HideProgress);

            this.IsPromptOpen = true;
            prompt.Show();
        }

        /// <summary>
        /// The can add review.
        /// </summary>
        /// <returns>
        /// The can add review.
        /// </returns>
        private bool CanAddReview()
        {
            return this.downloader.IsAuthenticated && this.mixData != null;
        }

        /// <summary>
        /// The can like unlike.
        /// </summary>
        /// <returns>
        /// The can like unlike.
        /// </returns>
        private bool CanLikeUnlike()
        {
            return this.downloader.IsAuthenticated && this.mixData != null;
        }

        /// <summary>
        /// The like unlike.
        /// </summary>
        private void LikeUnlike()
        {
            this.Mix.Liked = !this.Mix.Liked;
            this.UpdateLikedState();
            this.ShowProgress(StringResources.Progress_Loading);
            this.profileService.SetMixLikedAsync(this.MixId, this.Mix.Liked).ObserveOnDispatcher().Subscribe(
                _ =>
                {
                    // HACK: To force reloading the main liked page
                    MainPageLikedViewModel.ForceReload = true;
                }, this.HandleError, this.HideProgress);
        }

        /// <summary>
        /// Loads the current panel if it's not loaded (currently only the reviews panel is deferred)
        /// </summary>
        private void LoadCurrentPanel()
        {
            if (this.currentPanelIndex == 1 && !this.ReviewsPanel.IsLoaded)
            {
                this.AddToLifetime(this.ReviewsPanel.LoadAsync(this.MixId).Subscribe(_ => { }, this.HandleError));
            }
        }

        /// <summary>
        /// The load mix.
        /// </summary>
        /// <param name="loadMix">
        /// The load mix.
        /// </param>
        private void LoadMix(MixContract loadMix)
        {
            this.mixData = loadMix;
            this.CreatedByUserId = loadMix.User.Id;
            this.Mix = new MixViewModel(loadMix, this.censor);
            this.OnPropertyChanged("MixName");
            this.UpdateLikedState();

            this.UpdatePinnedState();
            this.ReviewMixCommand.RaiseCanExecuteChanged();
            this.ShareCommand.RaiseCanExecuteChanged();
            this.EmailCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// The load mix async.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        private IObservable<MixContract> LoadMixAsync(string id)
        {
            return MixesService.GetMixAsync(id).ObserveOnDispatcher().Do(this.LoadMix);
        }

        /// <summary>
        /// The pin to start.
        /// </summary>
        private void PinToStart()
        {
            if (ForegroundPinService.IsPinned(this.mixData))
            {
                ForegroundPinService.UnpinFromStart(this.mixData);
            }
            else
            {
                ForegroundPinService.PinToStart(this.mixData);
            }

            this.UpdatePinnedState();
        }

        /// <summary>
        /// The update is in progress.
        /// </summary>
        private void UpdateIsInProgress()
        {
            if (this.PlayedPanel.IsInProgress)
            {
                this.ShowProgress(this.PlayedPanel.InProgressMessage);
            }
            else if (this.ReviewsPanel.IsInProgress)
            {
                this.ShowProgress(this.ReviewsPanel.InProgressMessage);
            }
            else
            {
                this.HideProgress();
            }
        }

        /// <summary>
        /// The update liked state.
        /// </summary>
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
        /// The update pinned state.
        /// </summary>
        private void UpdatePinnedState()
        {
            if (BackgroundPinService.IsPinned(this.mixData))
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