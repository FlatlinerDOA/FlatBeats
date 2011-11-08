namespace FlatBeats
{
    using System;
    using System.Linq;

    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;

    using NavigationEventArgs = System.Windows.Navigation.NavigationEventArgs;

    /// <summary>
    /// </summary>
    public partial class PlayPage : PhoneApplicationPage
    {
        private ApplicationBarBinder appBarBinder;

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public PlayPage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public PlayPageViewModel ViewModel
        {
            get
            {
                return this.DataContext as PlayPageViewModel;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="e">
        /// </param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (this.appBarBinder == null)
            {
                this.appBarBinder = new ApplicationBarBinder(this, this.ViewModel);
            }

            this.ViewModel.MixId = this.NavigationContext.QueryString["mix"];
            this.ViewModel.PlayOnLoad = this.NavigationContext.QueryString.ContainsKey("play")
                                        && this.NavigationContext.QueryString["play"] == "true";
            this.ViewModel.Load();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            this.ViewModel.Unload();
            base.OnNavigatingFrom(e);
        }

        #endregion

        private void NavigationList_OnNavigation(object sender, Controls.NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }

        private void UserButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/UserProfilePage.xaml?userid=" + this.ViewModel.CreatedByUserId, UriKind.Relative));
        }
    }
}