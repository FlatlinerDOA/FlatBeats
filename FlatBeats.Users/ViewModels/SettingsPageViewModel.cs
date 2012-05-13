// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsPageViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The settings page view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.Users.ViewModels
{
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// The settings page view model.
    /// </summary>
    public sealed class SettingsPageViewModel : PageViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private readonly ProfileService profileService;

        /// <summary>
        /// </summary>
        private int currentPanelIndex;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public SettingsPageViewModel()
            : this(ProfileService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SettingsPageViewModel class.
        /// </summary>
        /// <param name="profileService">
        /// </param>
        public SettingsPageViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
            this.Title = "FLAT BEATS";
            this.Settings = new UserSettingsViewModel(this.profileService);
            this.Mixes = new UserProfileMixesViewModel(true, this.profileService);
            this.Liked = new UserProfileLikedMixesViewModel(true, this.profileService);
            this.MixFeed = new MixFeedViewModel(this.profileService);
            this.FollowedByUsers = new FollowedByUsersViewModel(true, this.profileService);
            this.FollowsUsers = new FollowsUsersViewModel(true, this.profileService);
            this.Tracks = new UserProfileTracksViewModel(true, this.profileService);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public int CurrentPanelIndex
        {
            get
            {
                return this.currentPanelIndex;
            }

            set
            {
                this.currentPanelIndex = value;
                this.LoadCurrentDataPanel(this.Settings.CurrentUserId);
            }
        }

        /// <summary>
        /// Gets FollowedByUsers.
        /// </summary>
        public FollowedByUsersViewModel FollowedByUsers { get; private set; }

        /// <summary>
        /// Gets FollowsUsers.
        /// </summary>
        public FollowsUsersViewModel FollowsUsers { get; private set; }

        /// <summary>
        /// Gets MixFeed.
        /// </summary>
        public MixFeedViewModel MixFeed { get; private set; }

        /// <summary>
        /// Gets Mixes.
        /// </summary>
        public UserProfileMixesViewModel Mixes { get; private set; }

        /// <summary>
        /// Gets liked mixes
        /// </summary>
        public UserProfileLikedMixesViewModel Liked { get; private set; }

        /// <summary>
        /// Gets Settings.
        /// </summary>
        public UserSettingsViewModel Settings { get; private set; }

        /// <summary>
        /// Gets the Tracks panel
        /// </summary>
        public UserProfileTracksViewModel Tracks { get; private set; }
        
        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        private ILifetime<string> CurrentDataPanel
        {
            get
            {
                switch (this.currentPanelIndex)
                {
                    case 0:
                        return null;
                    case 1:
                        return this.MixFeed;
                    case 2:
                        return this.Mixes;
                    case 3:
                        return this.Liked;
                    case 4:
                        return this.Tracks;
                    case 5:
                        return this.FollowsUsers;
                    case 6:
                        return this.FollowedByUsers;
                }

                return null;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        public override void Load()
        {
            var progress = new[]
                {
                    this.Settings.IsInProgressChanges, 
                    this.Mixes.IsInProgressChanges, 
                    this.Liked.IsInProgressChanges, 
                    this.Tracks.IsInProgressChanges,
                    this.FollowsUsers.IsInProgressChanges, 
                    this.FollowedByUsers.IsInProgressChanges, 
                    this.MixFeed.IsInProgressChanges
                };

            this.AddToLifetime(progress.Merge().Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(
                this.Settings.CurrentUserIdChanges.ObserveOnDispatcher().Subscribe(this.LoadCurrentDataPanel));
            this.AddToLifetime(
                this.Settings.LoadAsync().ObserveOnDispatcher().Subscribe(
                    _ => { }, this.HandleError, this.LoadCompleted));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The load panels.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        private void LoadCurrentDataPanel(string userId)
        {
            if (userId == null)
            {
                this.ResetPanels();
                return;
            }

            if (this.CurrentDataPanel != null)
            {
                if (!this.CurrentDataPanel.IsLoaded)
                {
                    this.AddToLifetime(this.CurrentDataPanel.LoadAsync(userId).Subscribe(_ => { }, this.HandleError));
                }
            }
        }

        /// <summary>
        /// The reset panels.
        /// </summary>
        private void ResetPanels()
        {
            this.MixFeed.Reset();
            this.Mixes.Reset();
            this.Liked.Reset();
            this.FollowedByUsers.Reset();
            this.FollowsUsers.Reset();
        }

        /// <summary>
        /// The update is in progress.
        /// </summary>
        private void UpdateIsInProgress()
        {
            if (this.Settings.IsInProgress)
            {
                this.ShowProgress(this.Settings.InProgressMessage);
                return;
            }

            if (this.MixFeed.IsInProgress)
            {
                this.ShowProgress(this.MixFeed.InProgressMessage);
                return;
            }

            if (this.Mixes.IsInProgress)
            {
                this.ShowProgress(this.Mixes.InProgressMessage);
                return;
            }

            if (this.Liked.IsInProgress)
            {
                this.ShowProgress(this.Liked.InProgressMessage);
                return;
            }

            if (this.FollowsUsers.IsInProgress)
            {
                this.ShowProgress(this.FollowsUsers.InProgressMessage);
                return;
            }

            if (this.FollowedByUsers.IsInProgress)
            {
                this.ShowProgress(this.FollowedByUsers.InProgressMessage);
                return;
            }

            this.HideProgress();
        }

        #endregion
    }
}