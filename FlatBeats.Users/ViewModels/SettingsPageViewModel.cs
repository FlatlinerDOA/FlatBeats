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
    using System.Windows;
    using System.Linq;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Flatliner.Phone;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;
    using System.Collections.ObjectModel;
    using FlatBeats.DataModel.Profile;
using System.Collections.Generic;

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

        private string preferredListText;

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
            this.LoginLabelText = StringResources.Command_Login;
            this.ResetLabelText = StringResources.Command_Logout;
            this.SignupLabelText = StringResources.Command_CreateAccount;
            this.UserNameLabelText = StringResources.Label_UserName;
            this.PasswordLabelText = StringResources.Label_Password;
            this.PlayNextMixText = StringResources.Label_PlayNextMix;
            this.PreferredListText = StringResources.Label_PreferredList;
            this.CensorshipEnabledText = StringResources.Label_CensorshipEnabled;
            this.PlayOverWifiOnlyText = StringResources.Label_PlayOverWifiOnly;
            this.CanLogin = false;
            this.IsLoggedIn = false;
            this.Mixes = new UserProfileMixesViewModel(true);
            this.MixFeed = new MixFeedViewModel();
            this.FollowedByUsers = new FollowedByUsersViewModel(true);
            this.FollowsUsers = new FollowsUsersViewModel(true);
            this.SignupCommand = new DelegateCommand(this.Signup);
            this.LoginCommand = new DelegateCommand(this.SignIn);
            this.ResetCommand = new DelegateCommand(this.SignOut);
            this.RegisterErrorHandler<ServiceException>(this.HandleSignInWebException);
            this.PreferredListChoices = new ObservableCollection<string>();
        }

        private ErrorMessage HandleSignInWebException(ServiceException ex)
        {
            switch (ex.StatusCode)
            {
                case 422:
                    return new ErrorMessage(StringResources.Error_SignInFailed_Title, StringResources.Error_SignInFailed_Message);
            }

            return new ErrorMessage(StringResources.Error_ServerUnavailable_Title, StringResources.Error_ServerUnavailable_Message);
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

        public MixFeedViewModel MixFeed { get; private set; }

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

        private bool censorshipEnabled;

        public bool CensorshipEnabled
        {
            get
            {
                return this.censorshipEnabled;
            }
            set
            {
                if (this.censorshipEnabled == value)
                {
                    return;
                }

                this.censorshipEnabled = value;
                this.OnPropertyChanged("CensorshipEnabled");
                this.SaveSettings();

            }
        }

        private string censorshipEnabledText;

        public string CensorshipEnabledText
        {
            get
            {
                return this.censorshipEnabledText;
            }
            set
            {
                if (this.censorshipEnabledText == value)
                {
                    return;
                }

                this.censorshipEnabledText = value;
                this.OnPropertyChanged("CensorshipEnabledText");
            }
        }

        private string playOverWifiOnlyText;

        public string PlayOverWifiOnlyText
        {
            get
            {
                return this.playOverWifiOnlyText;
            }
            set
            {
                if (this.playOverWifiOnlyText == value)
                {
                    return;
                }

                this.playOverWifiOnlyText = value;
                this.OnPropertyChanged("PlayOverWifiOnlyText");
            }
        }

        private void SaveSettings()
        {
            if (!this.isSettingsLoaded)
            {
                return;
            }

            var userSettings = UserSettings.Current;
            userSettings.CensorshipEnabled = this.CensorshipEnabled;
            userSettings.PlayOverWifiOnly = this.PlayOverWifiOnly;
            userSettings.PlayNextMix = this.PlayNextMix;
            userSettings.PreferredList = preferredListMap.FirstOrDefault(p => p.Value == this.PreferredList).Key;
            this.AddToLifetime(ProfileService.SaveSettingsAsync(userSettings).Subscribe(_ => { }, this.HandleError));
        }

        private string playNextMixText;

        public string PlayNextMixText
        {
            get
            {
                return this.playNextMixText;
            }
            set
            {
                if (this.playNextMixText == value)
                {
                    return;
                }

                this.playNextMixText = value;
                this.OnPropertyChanged("PlayNextMixText");
            }
        }

        private bool playNextMix;

        public bool PlayNextMix
        {
            get
            {
                return this.playNextMix;
            }
            set
            {
                if (this.playNextMix == value)
                {
                    return;
                }

                this.playNextMix = value;
                this.OnPropertyChanged("PlayNextMix");
            }
        }

        private bool playOverWifiOnly;

        public bool PlayOverWifiOnly
        {
            get
            {
                return this.playOverWifiOnly;
            }
            set
            {
                if (this.playOverWifiOnly == value)
                {
                    return;
                }

                this.playOverWifiOnly = value;
                this.OnPropertyChanged("PlayOverWifiOnly");
                this.SaveSettings();

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
        public string PreferredListText
        {
            get
            {
                return this.preferredListText;
            }

            set
            {
                if (this.preferredListText == value)
                {
                    return;
                }

                this.preferredListText = value;
                this.OnPropertyChanged("PreferredListText");
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
            this.AddToLifetime(this.Mixes.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.FollowsUsers.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.FollowedByUsers.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.MixFeed.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));

            this.CanLogin = true;
            if (this.IsDataLoaded)
            {
                this.LoadPanels(this.loginResponse);
                return;
            }

            this.LoadSettings();
            this.ResetPanels();
            this.ShowProgress(StringResources.Progress_Loading);
            this.AddToLifetime(ProfileService.LoadCredentialsAsync().ObserveOnDispatcher().Subscribe(
                this.LoadCredentials, this.HandleError, this.LoadCompleted));
        }

        private bool isSettingsLoaded;

        private readonly Dictionary<string, string> preferredListMap = new Dictionary<string, string>()
        {
            { PreferredLists.Liked, StringResources.PreferredLists_Liked },
            { PreferredLists.MixFeed, StringResources.PreferredLists_MixFeed },
            { PreferredLists.Created, StringResources.PreferredLists_Created }
        };

        private void LoadSettings()
        {
            this.isSettingsLoaded = false;

            this.PreferredListChoices.Clear();
            foreach (var item in preferredListMap)
            {
                this.PreferredListChoices.Add(item.Value);
            }
            
            var userSettings = UserSettings.Current;
            this.CensorshipEnabled = userSettings.CensorshipEnabled;
            this.PlayOverWifiOnly = userSettings.PlayOverWifiOnly;
            this.PlayNextMix = userSettings.PlayNextMix;
            this.PreferredList = this.preferredListMap[userSettings.PreferredList ?? PreferredLists.Created];
            this.isSettingsLoaded = true;
        }

        #endregion

        private string preferredList;

        public string PreferredList
        {
            get
            {
                return this.preferredList;
            }

            set
            {
                if (this.preferredList == value)
                {
                    return;
                }

                this.preferredList = value;
                this.OnPropertyChanged("PreferredList");
                this.SaveSettings();
            }
        }

        public ObservableCollection<string> PreferredListChoices { get; private set; }

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

        private UserLoginResponseContract loginResponse;

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
            var q = ProfileService.LoadUserTokenAsync();

            this.AddToLifetime(        
            q.ObserveOnDispatcher().Subscribe(this.LoadPanels, this.HandleError, this.HideProgress));
        }

        private void LoadPanels(UserLoginResponseContract user)
        {
            this.loginResponse = user;
            if (this.loginResponse == null || this.loginResponse.CurrentUser == null)
            {
                this.IsLoggedIn = false;
                return;
            }

            this.IsLoggedIn = true;
            this.CanLogin = false;

            this.AddToLifetime(this.Mixes.LoadAsync(this.loginResponse.CurrentUser.Id).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(this.FollowsUsers.LoadAsync(this.loginResponse.CurrentUser.Id).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(this.FollowedByUsers.LoadAsync(this.loginResponse.CurrentUser.Id).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.AddToLifetime(this.MixFeed.LoadAsync(this.loginResponse.CurrentUser.Id).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            
            this.MixFeed.LoadFirstPage();
            this.Mixes.LoadFirstPage();
            this.FollowsUsers.LoadFirstPage();
            this.FollowedByUsers.LoadFirstPage();
        }

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

        /// <summary>
        /// </summary>
        private void SignOut()
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
                        this.ResetPanels();
                        this.CanLogin = true;
                        this.IsLoggedIn = false;
                        this.HideProgress();
                    }));
        }

        private void ResetPanels()
        {
            this.Mixes.Reset();
            this.MixFeed.Reset();
            this.FollowedByUsers.Reset();
            this.FollowsUsers.Reset();
        }

        /// <summary>
        /// </summary>
        private void SignIn()
        {
            var creds = new UserCredentialsContract { UserName = this.UserName.Trim(), Password = this.Password };
            this.ShowProgress(StringResources.Progress_SigningIn);
            var q = ProfileService.AuthenticateAsync(creds);
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