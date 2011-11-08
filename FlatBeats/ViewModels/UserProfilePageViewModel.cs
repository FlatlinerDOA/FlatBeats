//--------------------------------------------------------------------------------------------------
// <copyright file="UserProfilePageViewModel.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class UserProfilePageViewModel : PageViewModel, IApplicationBarViewModel
    {
        /// <summary>
        /// Initializes a new instance of the UserProfilePageViewModel class.
        /// </summary>
        public UserProfilePageViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.LikedMixes = new ObservableCollection<MixViewModel>();
            this.FollowedBy = new ObservableCollection<UserContract>();
            this.Following = new ObservableCollection<UserContract>();
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
        }

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

        public override void Load()
        {
            this.subscription.Dispose();
            if (this.IsDataLoaded)
            {
                return;
            }

            this.ShowProgress();
            var loadProcess = from userProfile in this.LoadUserAsync()
                              from mixes in this.LoadMixesAsync()
                              from following in this.LoadFollowingAsync()
                              from followedBy in this.LoadFollowedByAsync()
                              select new Unit();
            this.subscription = loadProcess.ObserveOnDispatcher().Subscribe(_ => { }, this.ShowError, this.LoadCompleted);
        }

        public override void Unload()
        {
            this.subscription.Dispose();
        }

        private IObservable<Unit> LoadFollowedByAsync()
        {
            // TODO: Load followed by
            return Observable.Return(new Unit());
        }

        private IObservable<Unit> LoadFollowingAsync()
        {
            // TODO: Load following
            return Observable.Return(new Unit());
        }

        private IObservable<Unit> LoadMixesAsync()
        {
            var mixes = from response in ProfileService.GetUserMixes(this.UserId)
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                        select new MixViewModel(mix);
            return mixes.FlowIn()
                .ObserveOnDispatcher()
                .FirstDo(_ => this.Mixes.Clear())
                .Do(
                    this.Mixes.Add, 
                    this.ShowError, 
                    this.HideProgress).FinallySelect(() => new Unit());
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
                this.AvatarImageUrl = new Uri(userContract.Avatar.ImageUrl, UriKind.RelativeOrAbsolute);
            }

            this.UserName = userContract.Name;
        }

        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public ObservableCollection<MixViewModel> LikedMixes { get; private set; }

        public ObservableCollection<UserContract> Following { get; private set; }

        public ObservableCollection<UserContract> FollowedBy { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }
    }
}
