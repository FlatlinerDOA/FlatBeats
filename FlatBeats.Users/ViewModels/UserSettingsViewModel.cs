
namespace FlatBeats.Users.ViewModels
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
    using FlatBeats.Users.ViewModels;

    public sealed class UserSettingsViewModel : PanelViewModel
    {
        private readonly ProfileService profileService;

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

        public UserSettingsViewModel() : this(ProfileService.Instance)
        {
        }

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
            this.CensorshipEnabledText = StringResources.Label_CensorshipEnabled;
            this.PlayOverWifiOnlyText = StringResources.Label_PlayOverWifiOnly;
            this.CanLogin = false;
            this.IsLoggedIn = false;
            this.SignupCommand = new DelegateCommand(this.Signup);
            this.LoginCommand = new DelegateCommand(this.SignIn);
            this.ResetCommand = new DelegateCommand(this.SignOut);
            this.RegisterErrorHandler<ServiceException>(this.HandleSignInWebException);
            this.PreferredListChoices = new ObservableCollection<string>();
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

        private void SaveSettings()
        {
            if (!this.isSettingsLoaded)
            {
                return;
            }

            var userSettings = this.profileService.UserSettings;
            userSettings.CensorshipEnabled = this.CensorshipEnabled;
            userSettings.PlayOverWifiOnly = this.PlayOverWifiOnly;
            userSettings.PlayNextMix = this.PlayNextMix;
            userSettings.PreferredList = preferredListMap.FirstOrDefault(p => p.Value == this.PreferredList).Key;
            this.AddToLifetime(this.profileService.SaveSettingsAsync(userSettings).Subscribe(_ => { }, this.HandleError));
        }

        protected override void ShowErrorMessageOverride(ErrorMessage result)
        {
            if (result == null)
            {
                return;
            }

            MessageBox.Show(result.Message, result.Title, MessageBoxButton.OK);
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
            foreach (var item in this.preferredListMap)
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
                this.profileService.ResetAsync().ObserveOnDispatcher().Subscribe(
                    _ => { },
                    () =>
                    {
                        this.UserName = null;
                        this.Password = null;
                        this.currentUserIdChanges.OnNext(null);
                        // this.ResetPanels();
                        this.CanLogin = true;
                        this.IsLoggedIn = false;
                        this.HideProgress();
                    }));
        }

        private readonly Subject<string> currentUserIdChanges = new Subject<string>();
        public IObservable<string> CurrentUserIdChanges 
        { 
            get 
            {
                return this.currentUserIdChanges;
            } 
        }

        /// <summary>
        /// </summary>
        private void SignIn()
        {
            var creds = new UserCredentialsContract { UserName = this.UserName.Trim(), Password = this.Password };
            this.CanLogin = false;
            this.ShowProgress(StringResources.Progress_SigningIn);
            this.AddToLifetime(
                profileService.AuthenticateAsync(creds).ObserveOnDispatcher()
                    .Subscribe(
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
        private void Signup()
        {
            var task = new WebBrowserTask
            {
                Uri = new Uri("http://8tracks.com/signup", UriKind.Absolute)
            };
            task.Show();
        }

        public string CurrentUserId
        {
            get
            {
                return this.loginResponse != null && this.loginResponse.CurrentUser != null ? this.loginResponse.CurrentUser.Id : null;
            }
        }

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


        public IObservable<Unit> LoadAsync()
        {
            this.ShowProgress(StringResources.Progress_Loading);
            this.LoadSettings();
            return from creds in this.profileService.LoadCredentialsAsync().DefaultIfEmpty().ObserveOnDispatcher().Do(this.LoadCredentials)
                   from token in this.profileService.LoadUserTokenAsync().DefaultIfEmpty().ObserveOnDispatcher().Do(this.LoadLoginResponse)
                   select new Unit();
        }

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
    }
}
