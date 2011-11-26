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
            this.FollowedBy = new ObservableCollection<UserContract>();
            this.Following = new ObservableCollection<UserContract>();
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

            this.ShowProgress();
            var loadProcess = from userProfile in this.LoadUserAsync()
                              from mixes in this.Mixes.LoadMixesAsync(this.UserId)
                              ////from following in this.LoadFollowingAsync()
                              ////from followedBy in this.LoadFollowedByAsync()
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
            this.BioHtml = StripHtml(userContract.BioHtml);
            this.UserName = userContract.Name;
        }

        private string StripHtml(string htmlString)
        {
            //This pattern Matches everything found inside html tags;
            //(.|\n) - > Look for any character or a new line
            // *?  -> 0 or more occurences, and make a non-greedy search meaning
            //That the match will stop at the first available '>' it sees, and not at the last one
            //(if it stopped at the last one we could have overlooked 
            //nested HTML tags inside a bigger HTML tag..)
            // Thanks to Oisin and Hugh Brown for helping on this one...

            const string Pattern = @"<(.|\n)*?>";
            if (htmlString == null)
            {
                return string.Empty;
            }

            return Regex.Replace(htmlString, Pattern, string.Empty);
        }

        public ObservableCollection<MixViewModel> LikedMixes { get; private set; }

        public ObservableCollection<UserContract> Following { get; private set; }

        public ObservableCollection<UserContract> FollowedBy { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }
    }
}
