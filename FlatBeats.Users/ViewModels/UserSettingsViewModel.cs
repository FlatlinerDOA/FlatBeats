// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserSettingsViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Profile;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Flatliner.Phone;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;
    using Flatliner.Functional;

    /// <summary>
    /// </summary>
    public sealed class UserSettingsViewModel : PanelViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private readonly Subject<string> currentUserIdChanges = new Subject<string>();

        /// <summary>
        /// </summary>
        private readonly Dictionary<string, string> preferredListMap = new Dictionary<string, string> {
                { PreferredLists.Liked, StringResources.PreferredLists_Liked }, 
                { PreferredLists.MixFeed, StringResources.PreferredLists_MixFeed }, 
                { PreferredLists.Created, StringResources.PreferredLists_Created }
            };

        /// <summary>
        /// </summary>
        private readonly Dictionary<string, string> musicStoreMap = new Dictionary<string, string> {
                { MusicStores.WindowsPhone, StringResources.MusicStores_WindowsPhone }, 
                { MusicStores.NokiaMixRadio, StringResources.MusicStores_NokiaMixRadio }
            };

        /// <summary>
        /// </summary>
        private readonly ProfileService profileService;

        /// <summary>
        /// </summary>
        private bool canLogin;

        /// <summary>
        /// </summary>
        private bool censorshipEnabled;

        /// <summary>
        /// </summary>
        private string censorshipEnabledText;

        /// <summary>
        /// </summary>
        private bool isLoggedIn;

        /// <summary>
        /// </summary>
        private bool isSettingsLoaded;

        /// <summary>
        /// </summary>
        private string loginLabelText;

        /// <summary>
        /// </summary>
        private UserLoginResponseContract loginResponse;

        /// <summary>
        /// </summary>
        private string password;

        /// <summary>
        /// </summary>
        private string passwordLabelText;

        /// <summary>
        /// </summary>
        private bool playNextMix;

        /// <summary>
        /// </summary>
        private string playNextMixText;

        /// <summary>
        /// </summary>
        private bool playOverWifiOnly;

        /// <summary>
        /// </summary>
        private string playOverWifiOnlyText;

        /// <summary>
        /// </summary>
        private string preferredList = StringResources.PreferredLists_Liked;

        /// <summary>
        /// </summary>
        private string preferredListText;

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

        private string musicStore = StringResources.MusicStores_WindowsPhone;

        private string musicStoreText;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public UserSettingsViewModel()
            : this(ProfileService.Instance)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="profileService">
        /// </param>
        public UserSettingsViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
            this.Title = "settings";
            this.LoginLabelText = StringResources.Command_Login;
            this.ResetLabelText = StringResources.Command_Logout;
            this.SignupLabelText = StringResources.Command_CreateAccount;
            this.UserNameLabelText = StringResources.Label_UserName;
            this.PasswordLabelText = StringResources.Label_Password;
            this.PlayNextMixText = StringResources.Label_PlayNextMix;
            this.PreferredListText = StringResources.Label_PreferredList;
            this.MusicStoreText = StringResources.Label_MusicStore;
            this.CensorshipEnabledText = StringResources.Label_CensorshipEnabled;
            this.PlayOverWifiOnlyText = StringResources.Label_PlayOverWifiOnly;
            this.CanLogin = false;
            this.IsLoggedIn = false;
            this.SignupCommand = new DelegateCommand(this.Signup);
            this.LoginCommand = new DelegateCommand(this.SignIn, this.CanSignIn);
            this.ResetCommand = new DelegateCommand(this.SignOut, this.CanSignOut);
            this.RegisterErrorHandler<ServiceException>(this.HandleSignInWebException);
            this.PreferredListChoices = new ObservableCollection<string>();

            foreach (var item in this.preferredListMap)
            {
                this.PreferredListChoices.Add(item.Value);
            }

            this.MusicStoreChoices = new ObservableCollection<string>();
            foreach (var item in this.musicStoreMap)
            {
                this.MusicStoreChoices.Add(item.Value);
            }
        }

        public string MusicStoreText
        {
            get
            {
                return this.musicStoreText;
            }
            set
            {
                this.musicStoreText = value;
                this.OnPropertyChanged("MusicStoreText");
            }
        }

        private bool CanSignOut()
        {
            return this.IsLoggedIn;
        }

        private bool CanSignIn()
        {
            return this.CanLogin && !string.IsNullOrWhiteSpace(this.UserName)
                   && !string.IsNullOrWhiteSpace(this.Password);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
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
                this.LoginCommand.RaiseCanExecuteChanged();
                this.ResetCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// </summary>
        public string MusicStore
        {
            get
            {
                return this.musicStore;
            }

            set
            {
                if (this.musicStore == value)
                {
                    return;
                }

                this.musicStore = value;
                this.OnPropertyChanged("MusicStore");
                this.SaveSettings();
            }
        }

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
        public string CurrentUserId
        {
            get
            {
                return this.loginResponse != null && this.loginResponse.CurrentUser != null
                           ? this.loginResponse.CurrentUser.Id
                           : null;
            }
        }

        /// <summary>
        /// </summary>
        public IObservable<string> CurrentUserIdChanges
        {
            get
            {
                return this.currentUserIdChanges;
            }
        }

        /// <summary>
        /// </summary>
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
                this.LoginCommand.RaiseCanExecuteChanged();
                this.ResetCommand.RaiseCanExecuteChanged();
            }
        }

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
                this.LoginCommand.RaiseCanExecuteChanged();
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
                this.SaveSettings();
            }
        }

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
        public ObservableCollection<string> PreferredListChoices { get; private set; }

        public ObservableCollection<string> MusicStoreChoices { get; private set; }
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
                this.LoginCommand.RaiseCanExecuteChanged();
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

        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public IObservable<Unit> LoadAsync()
        {
            this.ShowProgress(StringResources.Progress_Loading);
            var load = from settings in this.profileService.GetSettingsAsync().ObserveOnDispatcher().Do(this.LoadSettings)
                   from creds in
                       this.profileService.LoadCredentialsAsync().DefaultIfEmpty().ObserveOnDispatcher().Do(
                           this.LoadCredentials)
                   from token in
                       this.profileService.LoadUserTokenAsync().DefaultIfEmpty().ObserveOnDispatcher().Do(
                           this.LoadLoginResponse)
                   select new Unit();

            return load.Do( _ => { }, this.HandleError, this.HideProgress);
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="result">
        /// </param>
        protected override void ShowErrorMessageOverride(ErrorMessage result)
        {
            if (result == null)
            {
                return;
            }

            MessageBox.Show(result.Message, result.Title, MessageBoxButton.OK);
        }

        /// <summary>
        /// </summary>
        /// <param name="ex">
        /// </param>
        /// <returns>
        /// </returns>
        private ErrorMessage HandleSignInWebException(ServiceException ex)
        {
            switch (ex.StatusCode)
            {
                case 422:
                    return new ErrorMessage(
                        StringResources.Error_SignInFailed_Title, StringResources.Error_SignInFailed_Message);
            }

            return new ErrorMessage(
                StringResources.Error_ServerUnavailable_Title, StringResources.Error_ServerUnavailable_Message);
        }

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
        }

        /// <summary>
        /// </summary>
        /// <param name="user">
        /// </param>
        private void LoadLoginResponse(UserLoginResponseContract user)
        {
            this.loginResponse = user;
            if (this.loginResponse == null || this.loginResponse.CurrentUser == null)
            {
                this.IsLoggedIn = false;
                this.CanLogin = true;
                return;
            }

            this.IsLoggedIn = true;
            this.CanLogin = false;
            this.currentUserIdChanges.OnNext(this.loginResponse.CurrentUser.Id);
        }

        /// <summary>
        /// </summary>
        /// <param name="userSettings">
        /// </param>
        private void LoadSettings(SettingsContract userSettings)
        {
            this.isSettingsLoaded = false;

            this.CensorshipEnabled = userSettings.CensorshipEnabled;
            this.PlayOverWifiOnly = userSettings.PlayOverWifiOnly;
            this.PlayNextMix = userSettings.PlayNextMix;
            this.PreferredList = this.preferredListMap[userSettings.PreferredList ?? PreferredLists.Created];
            this.MusicStore = this.musicStoreMap[userSettings.MusicStore ?? MusicStores.WindowsPhone];
            this.isSettingsLoaded = true;
        }

        /// <summary>
        /// </summary>
        private void SaveSettings()
        {
            if (!this.isSettingsLoaded)
            {
                return;
            }

            IObservable<PortableUnit> saveProcess = from userSettings in this.profileService.GetSettingsAsync().Do(
                s =>
                    {
                        s.CensorshipEnabled = this.CensorshipEnabled;
                        s.PlayOverWifiOnly = this.PlayOverWifiOnly;
                        s.PlayNextMix = this.PlayNextMix;
                        s.PreferredList = this.preferredListMap.FirstOrDefault(p => p.Value == this.PreferredList).Key;
                        s.MusicStore = this.musicStoreMap.FirstOrDefault(p => p.Value == this.musicStore).Key;
                    })
                                            from save in this.profileService.SaveSettingsAsync(userSettings)
                                            select ObservableEx.Unit;
            this.AddToLifetime(saveProcess.Subscribe(_ => { }, this.HandleError));
        }

        /// <summary>
        /// </summary>
        private void SignIn()
        {
            var creds = new UserCredentialsContract { UserName = this.UserName.Trim(), Password = this.Password };
            this.CanLogin = false;
            this.ShowProgress(StringResources.Progress_SigningIn);
            this.AddToLifetime(
                this.profileService.AuthenticateAsync(creds).ObserveOnDispatcher().Subscribe(
                    this.LoadLoginResponse, 
                    ex =>
                        {
                            this.CanLogin = true;
                            this.HandleError(ex);
                        }, 
                    this.HideProgress));
        }

        /// <summary>
        /// </summary>
        private void SignOut()
        {
            MessageBoxResult response = MessageBox.Show(
                StringResources.MessageBox_ResetSettings_Message, 
                StringResources.MessageBox_ResetSettings_Title, 
                MessageBoxButton.OKCancel);

            if (response == MessageBoxResult.Cancel)
            {
                return;
            }
            try
            {
                PlayState playState = BackgroundAudioPlayer.Instance.PlayerState;
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
            }
            catch (InvalidOperationException)
            {
                // Background audio resources no longer available
            }

            this.ShowProgress(StringResources.Progress_Loading);
            this.AddToLifetime(
                this.profileService.ResetAsync().ObserveOnDispatcher().Subscribe(
                    _ => { }, 
                    () =>
                        {
                            this.UserName = null;
                            this.Password = null;
                            this.loginResponse = null;
                            this.currentUserIdChanges.OnNext(null);

                            this.CanLogin = true;
                            this.IsLoggedIn = false;
                            this.HideProgress();
                        }));
        }

        /// <summary>
        /// </summary>
        private void Signup()
        {
            var task = new WebBrowserTask { Uri = new Uri("http://8tracks.com/signup", UriKind.Absolute) };
            task.Show();
        }

        #endregion
    }
}