namespace Flatliner.Phone.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Navigation;
    using Flatliner.Phone.Behaviors;
    using Flatliner.Portable;

    /// <summary>
    /// Base class for view models of pages.
    /// </summary>
    public abstract class AppBarPageViewModel : PageViewModel, IApplicationBarViewModel
    {
        #region Constants and Fields

        private string applicationTitle;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the PageViewModelBase class.
        /// </summary>
        protected AppBarPageViewModel()
        {
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
        }

        #endregion

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands
        {
            get;
            private set;
        }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands
        {
            get;
            private set;
        }

        public string ApplicationTitle
        {
            get
            {
                return this.applicationTitle;
            }

            set
            {
                if (this.applicationTitle == value)
                {
                    return;
                }

                this.applicationTitle = value;
                this.OnPropertyChanged(() => this.ApplicationTitle);
            }
        }

        public IDictionary<string, object> PageState
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the page state when the user has returned to this page.
        /// </summary>
        public virtual void RestorePageState()
        {
        }

        /// <summary>
        /// Saves the page state due to a user being taken out of the application (such as a 
        /// </summary>
        public virtual void SavePageState()
        {
            this.PageState.Clear();
        }
    }

    public abstract class PageViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the PageViewModel class.
        /// </summary>
        protected PageViewModel()
        {
            this.NavigationParameters = new Dictionary<string, string>(2);
        }

        public bool IsDataLoaded { get; private set; }

        private bool errorDisplayed;

        protected override void ShowErrorMessageOverride(ErrorMessage result)
        {
            if (result == null)
            {
                return;
            }

            if (result.IsCritical)
            {
                base.ShowErrorMessageOverride(result);
            }

            if (this.errorDisplayed)
            {
                return;
            }

            this.errorDisplayed = true;
            MessageBox.Show(result.Message, result.Title, MessageBoxButton.OK);
        }

        public NavigationService Navigation
        {
            get;
            set;
        }

        public IDictionary<string, string> NavigationParameters { get; set; }

        public IDictionary<string,object> State { get; set; }

        public abstract void Load();

        public override void Unload()
        {
            this.errorDisplayed = false;
            base.Unload();
        }

        protected void LoadCompleted()
        {
            this.HideProgress();
            this.IsDataLoaded = true;
        }
    }
}