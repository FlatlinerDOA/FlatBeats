namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class SettingsPageViewModel :PageViewModel, IApplicationBarViewModel
    {
        /// <summary>
        /// Initializes a new instance of the SettingsPageViewModel class.
        /// </summary>
        public SettingsPageViewModel()
        {
            this.Title = "PROFILE";
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>() { new CommandLink() { Command = new DelegateCommand(this.SignIn), Text = "sign in", IconUri = "/icons/appbar.check.rest.png"} };
        }

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }

        private void SignIn()
        {
            
            UserService.Authenticate(new UserCredentialsContract() { UserName =  this.UserName, Password = this.Password }).Subscribe();

        }

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

        private string password;

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
    }
}
