// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsPageViewModel.cs" company="">
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
    using System.Windows;

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
    public class SettingsPageViewModel : PageViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private string loginLabelText;

        /// <summary>
        /// </summary>
        private string password;

        /// <summary>
        /// </summary>
        private string passwordLabelText;

        /// <summary>
        /// </summary>
        private string resetLabelText;

        /// <summary>
        /// </summary>
        private string signupLabelText;

        /// <summary>
        /// </summary>
        private string userName;

        /// <summary>
        /// </summary>
        private string userNameLabelText;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the SettingsPageViewModel class.
        /// </summary>
        public SettingsPageViewModel()
        {
            this.Title = "PROFILE";
            this.LoginLabelText = "sign in";
            this.ResetLabelText = "sign out";
            this.SignupLabelText = "create account";
            this.UserNameLabelText = "Username";
            this.PasswordLabelText = "Password";
            this.CanLogin = false;
            this.IsLoggedIn = false;
            this.Mixes = new UserProfileMixesViewModel();
            this.FollowedByUsers = new FollowedByUsersViewModel();
            this.FollowsUsers = new FollowsUsersViewModel();
            this.SignupCommand = new DelegateCommand(this.Signup);
            this.LoginCommand = new DelegateCommand(this.SignIn);
            this.ResetCommand = new DelegateCommand(this.Reset);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public DelegateCommand LoginCommand { get; private set; }

        /// <summary>
        /// </summary>
        public string LoginLabelText
        {
            get
            {
                return this.loginLabelText;
            }

            set
            {
                if (this.loginLabelText == value)
                {
                    return;
                }

                this.loginLabelText = value;
                this.OnPropertyChanged("LoginLabelText");
            }
        }

        /// <summary>
        /// </summary>
        public UserProfileMixesViewModel Mixes { get; private set; }

        /// <summary>
        /// </summary>
        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                if (this.password == value)
                {
                    return;
                }

                this.password = value;
                this.OnPropertyChanged("Password");
            }
        }

        /// <summary>
        /// </summary>
        public string PasswordLabelText
        {
            get
            {
                return this.passwordLabelText;
            }

            set
            {
                if (this.passwordLabelText == value)
                {
                    return;
                }

                this.passwordLabelText = value;
                this.OnPropertyChanged("PasswordLabelText");
            }
        }

        /// <summary>
        /// </summary>
        public DelegateCommand ResetCommand { get; private set; }

        /// <summary>
        /// </summary>
        public string ResetLabelText
        {
            get
            {
                return this.resetLabelText;
            }

            set
            {
                if (this.resetLabelText == value)
                {
                    return;
                }

                this.resetLabelText = value;
                this.OnPropertyChanged("ResetLabelText");
            }
        }

        /// <summary>
        /// </summary>
        public DelegateCommand SignupCommand { get; private set; }

        /// <summary>
        /// </summary>
        public string SignupLabelText
        {
            get
            {
                return this.signupLabelText;
            }

            set
            {
                if (this.signupLabelText == value)
                {
                    return;
                }

                this.signupLabelText = value;
                this.OnPropertyChanged("SignupLabelText");
            }
        }

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

        /// <summary>
        /// </summary>
        public string UserNameLabelText
        {
            get
            {
                return this.userNameLabelText;
            }

            set
            {
                if (this.userNameLabelText == value)
                {
                    return;
                }

                this.userNameLabelText = value;
                this.OnPropertyChanged("UserNameLabelText");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public override void Load()
        {
            this.CanLogin = true;
            this.ShowProgress(StringResources.Progress_Loading);
            this.AddToLifetime(ProfileService.LoadCredentials().ObserveOnDispatcher().Subscribe(
                this.LoadCredentials, this.HandleError, this.LoadCompleted));
        }

        #endregion

        private bool isLoggedIn;

        public bool IsLoggedIn
        {
            get
            {
                return this.isLoggedIn;
            }
            set
            {
                if (this.isLoggedIn == value)
                {
                    return;
                }

                this.isLoggedIn = value;
                this.OnPropertyChanged("IsLoggedIn");
            }
        }

        private bool canLogin;

        public bool CanLogin
        {
            get
            {
                return this.canLogin;
            }
            set
            {
                if (this.canLogin == value)
                {
                    return;
                }

                this.canLogin = value;
                this.OnPropertyChanged("CanLogin");
            }
        }

        public FollowsUsersViewModel FollowsUsers { get; private set; }

        public FollowedByUsersViewModel FollowedByUsers { get; private set; }

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="creds">
        /// </param>
        private void LoadCredentials(UserCredentialsContract creds)
        {
            if (creds == null)
            {
                return;
            }

            this.CanLogin = false;
            this.UserName = creds.UserName;
            this.Password = creds.Password;

            this.ShowProgress(StringResources.Progress_Loading);
            var q = ProfileService.LoadUserToken();

            this.AddToLifetime(        
            q.Subscribe(
                this.LoadPanels, 
                this.HandleError, 
                this.HideProgress));
        }

        private void LoadPanels(UserLoginResponseContract user)
        {
            this.IsLoggedIn = true;
            this.CanLogin = false;

            this.AddToLifetime(this.Mixes.LoadAsync(user.CurrentUser.Id, true).Subscribe(_ => {}, this.HandleError, this.HideProgress));
            this.AddToLifetime(this.FollowsUsers.LoadAsync(user.CurrentUser.Id, true).Subscribe(_ => {}, this.HandleError, this.HideProgress));
            this.AddToLifetime(this.FollowedByUsers.LoadAsync(user.CurrentUser.Id, true).Subscribe(_ => { }, this.HandleError, this.HideProgress));

            this.Mixes.LoadNextPage();
            this.FollowsUsers.LoadNextPage();
            this.FollowedByUsers.LoadNextPage();
        }

        ////private IObservable<Unit> LoadMixes(string userId)
        ////{
        ////    var q = from mixes in ProfileService.GetUserMixes(userId)
        ////            from mix in mixes.Mixes.ToObservable(Scheduler.ThreadPool)
        ////            select new MixViewModel(mix);
                        
        ////    return q.FlowIn().ObserveOnDispatcher().FirstDo(_ => this.Mixes.Clear()).Do(
        ////        this.Mixes.Add, 
        ////        this.HandleError, 
        ////        () =>
        ////        {
        ////            if (this.Mixes.Count == 0)
        ////            {
        ////                this.Message = StringResources.Message_YouHaveNoMixes;
        ////                this.ShowMessage = true;
        ////            }
        ////            else
        ////            {
        ////                this.ShowMessage = false;
        ////            }
        ////        }).FinallySelect(() => new Unit());
        ////}

        /// <summary>
        /// </summary>
        private void Reset()
        {
            var response = MessageBox.Show(
                StringResources.MessageBox_ResetSettings_Message, 
                StringResources.MessageBox_ResetSettings_Title, 
                MessageBoxButton.OKCancel);

            if (response == MessageBoxResult.Cancel)
            {
                return;
            }

            var playState = BackgroundAudioPlayer.Instance.PlayerState;
            switch (playState)
            {
                case PlayState.Paused:
                case PlayState.Playing:
                case PlayState.BufferingStarted:
                case PlayState.BufferingStopped:
                case PlayState.TrackReady:
                case PlayState.TrackEnded:
                case PlayState.Rewinding:
                case PlayState.FastForwarding:
                    BackgroundAudioPlayer.Instance.Stop();
                    break;
                default:
                    break;
            }

            this.ShowProgress(StringResources.Progress_Loading);
            this.AddToLifetime(
                ProfileService.ResetAsync().ObserveOnDispatcher().Subscribe(
                    _ => { }, 
                    () =>
                    {
                        this.UserName = null;
                        this.Password = null;
                        this.Mixes.Items.Clear();
                        this.FollowedByUsers.Items.Clear();
                        this.FollowedByUsers.Items.Clear();
                        this.CanLogin = true;
                        this.IsLoggedIn = false;
                        this.HideProgress();
                    }));
        }

        /// <summary>
        /// </summary>
        private void SignIn()
        {
            var creds = new UserCredentialsContract { UserName = this.UserName, Password = this.Password };
            this.ShowProgress(StringResources.Progress_SigningIn);
            var q = ProfileService.Authenticate(creds);
            q.ObserveOnDispatcher()
                .Subscribe(
                this.LoadPanels, 
                this.HandleError, 
                this.HideProgress);
        }

        /// <summary>
        /// </summary>
        private void Signup()
        {
            var task = new WebBrowserTask
                {
                    Uri = new Uri("http://8tracks.com/signup", UriKind.Absolute)
                };
            task.Show();
        }

        #endregion
    }
}