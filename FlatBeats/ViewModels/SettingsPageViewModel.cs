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

    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class SettingsPageViewModel : PageViewModel, IApplicationBarViewModel
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
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>()
                {
                    new CommandLink
                        {
                            Command = new DelegateCommand(this.SignIn), 
                            Text = "sign in", 
                            IconUri = "/icons/appbar.check.rest.png"
                        }
                };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }

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

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public void Load()
        {
            this.ShowProgress();
            ProfileService.LoadCredentials().ObserveOnDispatcher().Subscribe(
                creds =>
                    {
                        if (creds != null)
                        {
                            this.UserName = creds.UserName;
                            this.Password = creds.Password;
                        }
                    }, 
                    this.ShowError, 
                    this.HideProgress);
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

        #endregion
    }
}