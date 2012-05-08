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
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// The settings page view model.
    /// </summary>
    public sealed class SettingsPageViewModel : PageViewModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the SettingsPageViewModel class.
        /// </summary>
        public SettingsPageViewModel()
        {
            this.Title = "FLAT BEATS";
            this.Settings = new UserSettingsViewModel();
            this.Mixes = new UserProfileMixesViewModel(true);
            this.MixFeed = new MixFeedViewModel();
            this.FollowedByUsers = new FollowedByUsersViewModel(true);
            this.FollowsUsers = new FollowsUsersViewModel(true);
        }

        #endregion

        #region Public Properties

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
        /// Gets Settings.
        /// </summary>
        public UserSettingsViewModel Settings { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        public override void Load()
        {
            var progress = new[]
                {
                    this.Mixes.IsInProgressChanges, 
                    this.FollowsUsers.IsInProgressChanges, 
                    this.FollowedByUsers.IsInProgressChanges, 
                    this.MixFeed.IsInProgressChanges
                };

            this.AddToLifetime(progress.Merge().Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.Settings.CurrentUserIdChanges.ObserveOnDispatcher().Subscribe(this.LoadPanels));
            this.AddToLifetime(this.Settings.LoadAsync().ObserveOnDispatcher().Subscribe(_ => { }, this.HandleError, this.LoadCompleted));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The load panels.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        private void LoadPanels(string userId)
        {
            this.AddToLifetime(this.Mixes.LoadAsync(userId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(
                this.FollowsUsers.LoadAsync(userId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(
                this.FollowedByUsers.LoadAsync(userId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(this.MixFeed.LoadAsync(userId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
        }

        /// <summary>
        /// The reset panels.
        /// </summary>
        private void ResetPanels()
        {
            this.Mixes.Reset();
            this.MixFeed.Reset();
            this.FollowedByUsers.Reset();
            this.FollowsUsers.Reset();
        }

        /// <summary>
        /// The update is in progress.
        /// </summary>
        private void UpdateIsInProgress()
        {
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