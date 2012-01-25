//--------------------------------------------------------------------------------------------------
// <copyright file="UserProfilePageViewModel.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;

    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Flatliner.Phone;
    using Flatliner.Phone.Data;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;

    public class UserProfilePageViewModel : PageViewModel, IApplicationBarViewModel
    {
        private static readonly Uri FollowUserIcon = new Uri("/icons/appbar.game.addfriend.rest.png", UriKind.Relative);

        private static readonly Uri UnfollowUserIcon = new Uri("/icons/appbar.game.removefriend.rest.png", UriKind.Relative);

        /// <summary>
        /// Initializes a new instance of the UserProfilePageViewModel class.
        /// </summary>
        public UserProfilePageViewModel()
        {
            this.Mixes = new UserProfileMixesViewModel(false);
            this.LikedMixes = new ObservableCollection<MixViewModel>();
            this.FollowedByUsers = new FollowedByUsersViewModel(false);
            this.FollowsUsers = new FollowsUsersViewModel(false);
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

        private bool CanToggleFollowUser()
        {
            return Downloader.IsAuthenticated;
        }

        private void ToggleFollowUser()
        {
            this.ShowProgress(StringResources.Progress_Updating);
            ProfileService.SetFollowUser(this.UserId, !this.IsCurrentUserFollowing).ObserveOnDispatcher().Subscribe(
                response =>
                    {
                        this.HideProgress();
                        this.IsCurrentUserFollowing = response.User.IsFollowed;
                    });
        }

        private bool isCurrentUserFollowing;

        protected bool IsCurrentUserFollowing
        {
            get
            {
                return this.isCurrentUserFollowing;
            }

            set
            {
                this.isCurrentUserFollowing = value;
                this.ToggleFollowUserCommandLink.IconUrl = this.IsCurrentUserFollowing ? UnfollowUserIcon : FollowUserIcon;
                this.ToggleFollowUserCommandLink.Text = this.IsCurrentUserFollowing ? StringResources.Command_UnfollowUser : StringResources.Command_FollowUser;
                this.ToggleFollowUserCommandLink.RaiseCanExecuteChanged();
            }
        }

        public CommandLink ToggleFollowUserCommandLink { get; private set; }

        public UserProfileMixesViewModel Mixes { get; private set; }

        public string UserId { get; set; }

        private string userName;

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

        private Uri avatarImageUrl;


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

        private string bioHtml;

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

        private string location;

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

        public override void Load()
        {
            if (this.IsDataLoaded)
            {
                return;
            }

            this.UserId = this.NavigationParameters["userid"];

            this.ShowProgress(StringResources.Progress_Loading);
            this.AddToLifetime(this.LoadUserAsync().ObserveOnDispatcher().Subscribe(_ => this.LoadPanels(), this.HandleError, this.LoadCompleted));
        }

        private void LoadPanels()
        {
            this.AddToLifetime(this.Mixes.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.FollowsUsers.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.FollowedByUsers.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));

            this.AddToLifetime(
                this.Mixes.LoadAsync(this.UserId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(
                this.FollowsUsers.LoadAsync(this.UserId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(
                this.FollowedByUsers.LoadAsync(this.UserId).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.Mixes.LoadNextPage();
            this.FollowsUsers.LoadNextPage();
            this.FollowedByUsers.LoadNextPage();
        }

        private void UpdateIsInProgress()
        {
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

        private IObservable<Unit> LoadUserAsync()
        {
            var profile = from response in ProfileService.GetUserProfile(this.UserId)
                          select response.User;
            return profile.ObserveOnDispatcher().Do(this.LoadUserProfile).FinallySelect(() => new Unit());
        }

        private void LoadUserProfile(UserContract userContract)
        {
            if (userContract.Avatar != null)
            {
                this.AvatarImageUrl = new Uri(userContract.Avatar.LargeImageUrl, UriKind.RelativeOrAbsolute);
            }

            this.Location = userContract.Location;
            this.BioHtml = Html.ConvertToPlainText(userContract.BioHtml);
            this.UserName = userContract.Name;
            this.IsCurrentUserFollowing = userContract.IsFollowed;
        }

        public ObservableCollection<MixViewModel> LikedMixes { get; private set; }

        public FollowsUsersViewModel FollowsUsers { get; private set; }

        public FollowedByUsersViewModel FollowedByUsers { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }
    }
}
