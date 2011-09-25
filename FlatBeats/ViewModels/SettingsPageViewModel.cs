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

    /// <summary>
    /// </summary>
    public class SettingsPageViewModel : PageViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private string password;

        /// <summary>
        /// </summary>
        private string userName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the SettingsPageViewModel class.
        /// </summary>
        public SettingsPageViewModel()
        {
            this.Title = "PROFILE";
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.LoginCommand = new DelegateCommand(this.SignIn);
            this.ResetCommand = new DelegateCommand(this.Reset);
        }

        #endregion

        #region Public Properties

        public DelegateCommand LoginCommand { get; private set; }

        public DelegateCommand ResetCommand { get; private set; }
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

        public ObservableCollection<MixViewModel> Mixes { get; private set; }
        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public void Load()
        {
            this.ShowProgress();
            ProfileService.LoadCredentials().ObserveOnDispatcher().Subscribe(
                    this.LoadProfile, 
                    this.ShowError, 
                    this.HideProgress);
        }

        private void LoadProfile(UserCredentialsContract creds)
        {
            if (creds == null)
            {
                return;
            }

            this.UserName = creds.UserName;
            this.Password = creds.Password;

            var q = from user in ProfileService.LoadUserToken()
                    from mixes in ProfileService.GetUserMixes(user.CurentUser.Id)
                    from mix in
                        mixes.Mixes.ToObservable().Select(m => new MixViewModel(m)).ObserveOnDispatcher().FirstDo(
                            _ => this.Mixes.Clear()).Do(this.Mixes.Add)
                    select new Unit();
            q.Subscribe(_ => { }, this.ShowError, this.ShowProgress);
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        private void SignIn()
        {
            var creds = new UserCredentialsContract { UserName = this.UserName, Password = this.Password };
            this.ShowProgress();
            ProfileService.Authenticate(creds).ObserveOnDispatcher().Subscribe(_ => { }, this.ShowError, this.HideProgress);
        }

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
        }

        #endregion
    }
}