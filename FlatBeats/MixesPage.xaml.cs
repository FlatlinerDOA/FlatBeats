namespace FlatBeats
{
    using System;
    using Microsoft.Phone.Controls;
    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    public partial class MixesPage : PhoneApplicationPage
    {
        public MixesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string tag = this.NavigationContext.QueryString["tag"];
            this.Dispatcher.BeginInvoke(new Action(() => this.ViewModel.Load(tag)));
        }

        public MixesPageViewModel ViewModel 
        { 
            get
            {
                return (MixesPageViewModel)this.DataContext;
            } 
        }

        private void NavigationList_OnNavigation(object sender, NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }

        private void pivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.ViewModel.CurrentPanelIndex = pivot.SelectedIndex;
        }
    }
}