namespace FlatBeats
{
    using System;
    using System.Windows.Navigation;

    using Microsoft.Phone.Controls;

    using FlatBeats.ViewModels;

    using NavigationEventArgs = FlatBeats.Controls.NavigationEventArgs;

    public partial class MixesPage : PhoneApplicationPage
    {
        public MixesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string query, tag;
            this.NavigationContext.QueryString.TryGetValue("tag", out tag);
            this.NavigationContext.QueryString.TryGetValue("q", out query);

            this.Dispatcher.BeginInvoke(new Action(() => this.ViewModel.Load(tag, query)));
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

        private void PivotSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.ViewModel.CurrentPanelIndex = pivot.SelectedIndex;
        }
    }
}