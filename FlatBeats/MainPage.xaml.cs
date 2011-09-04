namespace FlatBeats
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;

    using Coding4Fun.Phone.Controls;

    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Reactive;

    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            this.InitializeComponent();

            // Set the data context of the listbox control to the sample data
            this.DataContext = App.ViewModel;
            this.Loaded += this.MainPage_Loaded;
            this.Unloaded += this.MainPage_Unloaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Unload();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.Dispatcher.BeginInvoke(new Action(App.ViewModel.Load));
        }

        private void NavigationList_OnNavigation(object sender, NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }

        private void pano_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (pano.SelectedIndex != 0)
            {
                HubTileService.FreezeGroup("History");
            }
            else
            {
                HubTileService.UnfreezeGroup("History");
            }

            if (pano.SelectedIndex != 1)
            {
                HubTileService.FreezeGroup("Latest");
            }
            else
            {
                HubTileService.UnfreezeGroup("Latest");
            }

            App.ViewModel.CurrentSectionIndex = pano.SelectedIndex;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var search = new SearchPrompt();
            Observable.FromEvent<PopUpEventArgs<string, PopUpResult>>(search, "Completed").Take(1).ObserveOnDispatcher()
                .Subscribe(
                    q =>
                        {
                            if (q.EventArgs.PopUpResult == PopUpResult.Ok)
                            {
                                this.NavigationService.Navigate(
                                    new Uri("/MixesPage.xaml?q=" + Uri.EscapeDataString(q.EventArgs.Result), UriKind.Relative));
                            }
                        });
            search.Title = "search";
            search.Show();
            
        }
    }
}