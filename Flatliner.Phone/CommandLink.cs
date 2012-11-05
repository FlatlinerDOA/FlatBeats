namespace Flatliner.Phone
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using Flatliner.Phone.ViewModels;
    using Flatliner.Portable;

    public class CommandLink : ViewModel, ICommandLink
    {
        /// <summary>
        /// Initializes a new instance of the CommandLink class.
        /// </summary>
        public CommandLink()
        {
            this.Visibility = Visibility.Visible;
        }

        #region Constants and Fields

        private ICommand command;

        private bool hideWhenInactive;

        private Uri iconUrl;

        private string text;

        private Visibility visibility;

        #endregion

        #region Public Properties

        public ICommand Command
        {
            get
            {
                return this.command;
            }
            set
            {
                if (this.command != null)
                {
                    this.command.CanExecuteChanged -= this.CommandCanExecuteChanged;
                }

                this.command = value;
                if (this.command != null)
                {
                    this.command.CanExecuteChanged += this.CommandCanExecuteChanged;
                }
            }
        }

        public bool HideWhenInactive
        {
            get
            {
                return this.hideWhenInactive;
            }
            set
            {
                this.hideWhenInactive = value;
                this.UpdateVisibility();
            }
        }

        public Uri IconUrl
        {
            get
            {
                return this.iconUrl;
            }
            set
            {
                if (this.iconUrl == value)
                {
                    return;
                }

                this.iconUrl = value;
                this.OnPropertyChanged("IconUrl");
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (this.text == value)
                {
                    return;
                }

                this.text = value;
                this.OnPropertyChanged("Text");
            }
        }

        public Visibility Visibility
        {
            get
            {
                return this.visibility;
            }
            set
            {
                if (this.visibility == value)
                {
                    return;
                }

                this.visibility = value;
                this.OnPropertyChanged("Visibility");
            }
        }

        #endregion

        #region Public Methods

        public void RaiseCanExecuteChanged()
        {
            var cmd = this.Command as DelegateCommand;
            if (cmd != null)
            {
                cmd.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Methods

        private void CommandCanExecuteChanged(object sender, EventArgs e)
        {
            this.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (this.HideWhenInactive)
            {
                this.Visibility = this.Command.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}