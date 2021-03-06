﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserProfilePageViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Flatliner.Phone;
    using Flatliner.Phone.Data;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;

    using PageViewModel = FlatBeats.ViewModels.PageViewModel;

    /// <summary>
    /// </summary>
    public class UserProfilePageViewModel : PageViewModel, IApplicationBarViewModel
    {
        private readonly IAsyncDownloader downloader;

        private readonly ProfileService profileService;

        #region Constants and Fields

        /// <summary>
        /// </summary>
        private static readonly Uri FollowUserIcon = new Uri("/icons/appbar.game.addfriend.rest.png", UriKind.Relative);

        /// <summary>
        /// </summary>
        private static readonly Uri UnfollowUserIcon = new Uri(
            "/icons/appbar.game.removefriend.rest.png", UriKind.Relative);

        /// <summary>
        /// </summary>
        private Uri avatarImageUrl;

        /// <summary>
        /// </summary>
        private string bioHtml;

        /// <summary>
        /// </summary>
        private bool isCurrentUserFollowing;

        /// <summary>
        /// </summary>
        private string location;

        /// <summary>
        /// </summary>
        private string userName;

        private bool censor;

        #endregion

        #region Constructors and Destructors

        public UserProfilePageViewModel() : this(AsyncDownloader.Instance, ProfileService.Instance)
        {
            
        }

        /// <summary>
        ///   Initializes a new instance of the UserProfilePageViewModel class.
        /// </summary>
        public UserProfilePageViewModel(IAsyncDownloader downloader, ProfileService profileService)
        {
            this.downloader = downloader;
            this.profileService = profileService;
            this.Mixes = new UserProfileMixesViewModel(false, this.profileService);
            this.LikedMixes = new UserProfileLikedMixesViewModel(false, this.profileService);
            this.Tracks = new UserProfileTracksViewModel(false, this.profileService);
            this.FollowedByUsers = new FollowedByUsersViewModel(false, this.profileService);
            this.FollowsUsers = new FollowsUsersViewModel(false, this.profileService);
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
            this.ToggleFollowUserCommandLink = new CommandLink()
                {
                    Command = new DelegateCommand(this.ToggleFollowUser, this.CanToggleFollowUser), 
                    Text = StringResources.Command_FollowUser, 
                    IconUrl = FollowUserIcon
                };
            this.ApplicationBarButtonCommands.Add(this.ToggleFollowUserCommandLink);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Private property backing field
        /// </summary>
        private int currentPanelIndex;

        /// <summary>
        /// Gets or sets the index of the current panel being displayed
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
                this.OnPropertyChanged(() => this.CurrentPanelIndex);
                this.LoadCurrentPanel();
            }
        }

        /// <summary>
        /// </summary>
        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }

        /// <summary>
        /// </summary>
        public Uri AvatarImageUrl
        {
            get
            {
                return this.avatarImageUrl;
            }

            set
            {
                if (this.avatarImageUrl == value)
                {
                    return;
                }

                this.avatarImageUrl = value;
                this.OnPropertyChanged("AvatarImageUrl");
            }
        }

        /// <summary>
        /// </summary>
        public string BioHtml
        {
            get
            {
                return this.bioHtml;
            }

            set
            {
                if (this.bioHtml == value)
                {
                    return;
                }

                this.bioHtml = value;
                this.OnPropertyChanged("BioHtml");
            }
        }

        /// <summary>
        /// </summary>
        public FollowedByUsersViewModel FollowedByUsers { get; private set; }

        /// <summary>
        /// </summary>
        public FollowsUsersViewModel FollowsUsers { get; private set; }

        /// <summary>
        /// </summary>
        public UserProfileLikedMixesViewModel LikedMixes { get; private set; }

        /// <summary>
        /// </summary>
        public string Location
        {
            get
            {
                return this.location;
            }

            set
            {
                if (this.location == value)
                {
                    return;
                }

                this.location = value;
                this.OnPropertyChanged("Location");
            }
        }

        /// <summary>
        /// </summary>
        public UserProfileMixesViewModel Mixes { get; private set; }

        /// <summary>
        /// </summary>
        public UserProfileTracksViewModel Tracks { get; private set; }

        /// <summary>
        /// </summary>
        public CommandLink ToggleFollowUserCommandLink { get; private set; }

        /// <summary>
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// </summary>
        public string UserName
        {
            get
            {
                return this.userName;
            }

            set
            {
                if (this.userName == value)
                {
                    return;
                }

                this.userName = value;
                this.OnPropertyChanged("UserName");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        protected bool IsCurrentUserFollowing
        {
            get
            {
                return this.isCurrentUserFollowing;
            }

            set
            {
                this.isCurrentUserFollowing = value;
                this.ToggleFollowUserCommandLink.IconUrl = this.IsCurrentUserFollowing
                                                               ? UnfollowUserIcon
                                                               : FollowUserIcon;
                this.ToggleFollowUserCommandLink.Text = this.IsCurrentUserFollowing
                                                            ? StringResources.Command_UnfollowUser
                                                            : StringResources.Command_FollowUser;
                this.ToggleFollowUserCommandLink.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public override void Load()
        {
            if (this.NavigationParameters.ContainsKey("username"))
            {
                this.UserName = this.NavigationParameters["username"];
            }

            var progressChanges = new[]
                {
                    this.Mixes.IsInProgressChanges,
                    this.LikedMixes.IsInProgressChanges,
                    this.FollowsUsers.IsInProgressChanges,
                    this.FollowedByUsers.IsInProgressChanges
                };
            this.AddToLifetime(progressChanges.Merge().Subscribe(_ => this.UpdateIsInProgress()));

            if (this.IsDataLoaded)
            {
                if (this.UserId != null)
                {
                    this.LoadCurrentPanel();
                }
                return;
            }

            this.UserId = this.NavigationParameters["userid"];
            this.ShowProgress(StringResources.Progress_Loading);
            this.AddToLifetime(
                this.LoadUserAsync().ObserveOnDispatcher().Subscribe(
                    _ => this.LoadCurrentPanel(), this.HandleError, this.LoadCompleted));
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private bool CanToggleFollowUser()
        {
            return this.downloader.IsAuthenticated;
        }

        /// <summary>
        /// </summary>
        private void LoadCurrentPanel()
        {
            switch (this.CurrentPanelIndex)
            {
                case 1:
                    this.AddToLifetime(this.Mixes.LoadAsync(this.UserId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
                    break;
                case 2:
                    this.AddToLifetime(this.LikedMixes.LoadAsync(this.UserId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
                    break;
                case 3:
                    this.AddToLifetime(this.Tracks.LoadAsync(this.UserId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
                    break;
                case 4:
                    this.AddToLifetime(this.FollowsUsers.LoadAsync(this.UserId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
                    break;
                case 5:
                    this.AddToLifetime(this.FollowedByUsers.LoadAsync(this.UserId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
                    break;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private IObservable<Unit> LoadUserAsync()
        {
            var profile = from settings in this.profileService.GetSettingsAsync().Do(s => this.censor = s.CensorshipEnabled)
                from response in this.profileService.GetUserProfileAsync(this.UserId) 
                          select response.User;
            return profile.ObserveOnDispatcher().Do(this.LoadUserProfile).FinallySelect(() => new Unit());
        }

        /// <summary>
        /// </summary>
        /// <param name="userContract">
        /// </param>
        private void LoadUserProfile(UserContract userContract)
        {
            this.AvatarImageUrl = Avatar.GetLargeImageUrl(userContract.Avatar);
            this.Location = userContract.Location;
            var text = Html.ConvertToPlainText(userContract.BioHtml).Trim();
            this.BioHtml = this.censor ? Censorship.Censor(text) : text;
            this.UserName = userContract.Name;
            this.IsCurrentUserFollowing = userContract.IsFollowed;
        }

        /// <summary>
        /// </summary>
        private void ToggleFollowUser()
        {
            this.ShowProgress(StringResources.Progress_Updating);
            this.profileService.SetFollowUserAsync(this.UserId, !this.IsCurrentUserFollowing).ObserveOnDispatcher().Subscribe(
                response =>
                    {
                        this.HideProgress();
                        this.IsCurrentUserFollowing = response.User.IsFollowed;
                    });
        }

        /// <summary>
        /// </summary>
        private void UpdateIsInProgress()
        {
            if (this.Mixes.IsInProgress)
            {
                this.ShowProgress(this.Mixes.InProgressMessage);
                return;
            }

            if (this.LikedMixes.IsInProgress)
            {
                this.ShowProgress(this.LikedMixes.InProgressMessage);
                return;
            }

            if (this.Tracks.IsInProgress)
            {
                this.ShowProgress(this.Tracks.InProgressMessage);
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