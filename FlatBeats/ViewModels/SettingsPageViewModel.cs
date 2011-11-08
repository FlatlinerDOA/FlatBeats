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

        private IDisposable subscription = Disposable.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the SettingsPageViewModel class.
        /// </summary>
        public SettingsPageViewModel()
        {
            this.Title = "PROFILE";
            this.LoginLabelText = "login";
            this.ResetLabelText = "reset";
            this.SignupLabelText = "create account";
            this.UserNameLabelText = "Username";
            this.PasswordLabelText = "Password";
            this.Mixes = new ObservableCollection<MixViewModel>();
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
        public ObservableCollection<MixViewModel> Mixes { get; private set; }

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
            this.subscription.Dispose();
            this.ShowProgress();
            this.subscription = ProfileService.LoadCredentials().ObserveOnDispatcher().Subscribe(
                this.LoadProfile, this.ShowError, this.LoadCompleted);
        }

        public override void Unload()
        {
            this.subscription.Dispose();
        }
        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="creds">
        /// </param>
        private void LoadProfile(UserCredentialsContract creds)
        {
            if (creds == null)
            {
                return;
            }

            this.UserName = creds.UserName;
            this.Password = creds.Password;

            this.ShowProgress();
            var q = from user in ProfileService.LoadUserToken()
                    from loaded in this.LoadMixes(user.CurentUser.Id)
                    select new Unit();
            q.Subscribe(_ => { }, this.ShowError, this.HideProgress);
        }

        private IObservable<Unit> LoadMixes(string userId)
        {
            var q = from mixes in ProfileService.GetUserMixes(userId)
                    from mix in mixes.Mixes.ToObservable(Scheduler.ThreadPool)
                    select new MixViewModel(mix);
                        
            return q.FlowIn().ObserveOnDispatcher().FirstDo(_ => this.Mixes.Clear()).Do(this.Mixes.Add).FinallySelect(() => new Unit());
        }

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

            ProfileService.Reset();
            this.UserName = null;
            this.Password = null;
            this.Mixes.Clear();
        }

        /// <summary>
        /// </summary>
        private void SignIn()
        {
            var creds = new UserCredentialsContract { UserName = this.UserName, Password = this.Password };
            this.ShowProgress();
            var q = from auth in ProfileService.Authenticate(creds)
                    from mixLoaded in this.LoadMixes(auth.CurentUser.Id)
                    select new Unit();
            q.ObserveOnDispatcher().Subscribe(_ => {  }, this.ShowError, this.HideProgress);
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