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
        /// <summary>
        /// Initializes a new instance of the UserProfilePageViewModel class.
        /// </summary>
        public UserProfilePageViewModel()
        {
            this.Mixes = new UserProfileMixesViewModel();
            this.LikedMixes = new ObservableCollection<MixViewModel>();
            this.FollowedByUsers = new FollowedByUsersViewModel();
            this.FollowsUsers = new FollowsUsersViewModel();
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
        }

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

        private IDisposable subscription = Disposable.Empty;

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
            this.subscription.Dispose();
            if (this.IsDataLoaded)
            {
                return;
            }

            this.ShowProgress(StringResources.Progress_Loading);
            this.AddToLifetime(
            this.LoadUserAsync().ObserveOnDispatcher().Subscribe(_ => this.LoadPanels(), this.HandleError, this.LoadCompleted));
        }

        private void LoadPanels()
        {
            this.AddToLifetime(
                this.Mixes.LoadAsync(this.UserId, false).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(
                this.FollowsUsers.LoadAsync(this.UserId, false).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(
                this.FollowedByUsers.LoadAsync(this.UserId, false).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.Mixes.LoadNextPage();
            this.FollowsUsers.LoadNextPage();
            this.FollowedByUsers.LoadNextPage();
        }

        public override void Unload()
        {
            this.subscription.Dispose();
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
        }

        public ObservableCollection<MixViewModel> LikedMixes { get; private set; }

        public FollowsUsersViewModel FollowsUsers { get; private set; }

        public FollowedByUsersViewModel FollowedByUsers { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }
    }
}
