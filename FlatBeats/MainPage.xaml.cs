namespace FlatBeats
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;
    using Clarity.Phone.Extensions;

    using Coding4Fun.Phone.Controls;

    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Reactive;

    using GestureEventArgs = System.Windows.Input.GestureEventArgs;

    public partial class MainPage : AnimatedBasePage
    {
        private string PlayMixKey = "MixId";

        private bool historyItemLaunch;

        // Constructor
        public MainPage()
        {
            this.InitializeComponent();

            this.AnimationContext = this.LayoutRoot;

            // Set the data context of the listbox control to the sample data
            this.DataContext = App.ViewModel;
            this.Loaded += this.MainPage_Loaded;
            this.Unloaded += this.MainPage_Unloaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            LittleWatson.CheckForPreviousException();
        }

        public MainPageViewModel ViewModel
        {
            get
            {
                return (MainPageViewModel)this.DataContext;
            }
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (toOrFrom != null)
            {
                if (toOrFrom.OriginalString.Contains("PlayPage.xaml"))
                {
                    return null;
                    ////var container = (PanoramaItem)this.pano.ItemContainerGenerator.ContainerFromIndex(this.pano.SelectedIndex);
                    ////var listBox = ((FrameworkElement)container.Content).GetVisualChildren<ListBox>().FirstOrDefault();
                    ////if (listBox != null)
                    ////{
                    ////    var listItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(listBox.SelectedIndex);
                    ////    return this.GetContinuumAnimation(listItem, animationType);
                    ////}
                }
            }

            return null; 
            if (animationType == AnimationType.NavigateForwardIn)
            {
                return new TurnstileForwardInAnimator() { RootElement = this.LayoutRoot };
            }

            if (animationType == AnimationType.NavigateForwardOut)
            {
                return new TurnstileForwardOutAnimator() { RootElement = this.LayoutRoot };
            }

            if (animationType == AnimationType.NavigateBackwardIn)
            {
                return new TurnstileBackwardInAnimator() { RootElement = this.LayoutRoot };
            }

            return new TurnstileBackwardOutAnimator() { RootElement = this.LayoutRoot };
        }


        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= this.MainPage_Unloaded;
            App.ViewModel.Unload();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.ContainsKey(PlayMixKey))
            {
                // We were launched from a history item.
                // Change _playingSong even if something was already playing 
                // because the user directly chose a song history item.

                // Use the navigation context to find the song by name.
                string mixId = NavigationContext.QueryString[PlayMixKey];

                this.NavigationService.Navigate(new Uri("/PlayPage.xaml?mix=" + mixId + "&play=true", UriKind.Relative));

                // Set a flag to indicate that we were started from a 
                // history item and that we should immediately start 
                // playing the song once the UI has finished loading.
                this.historyItemLaunch = true;
                return;
            }


            this.Dispatcher.BeginInvoke(new Action(App.ViewModel.Load));
        }

        private void NavigationList_OnNavigation(object sender, NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
            this.NavigationService.NavigateTo(navItem);
        }

        private void pano_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ////if (pano.SelectedIndex != 1 || this.ViewModel.IsInProgress)
            ////{
            ////    HubTileService.FreezeGroup("History");
            ////}
            ////else
            ////{
            ////    HubTileService.UnfreezeGroup("History");
            ////}

            if (pano.SelectedIndex != 2 || this.ViewModel.IsInProgress)
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
                            if (q.EventArgs.PopUpResult == PopUpResult.Ok && !string.IsNullOrWhiteSpace(q.EventArgs.Result))
                            {
                                this.NavigationService.Navigate(
                                    new Uri(
                                        "/MixesPage.xaml?q=" + Uri.EscapeDataString(q.EventArgs.Result), 
                                        UriKind.Relative));
                            }
                        });
            
            search.ActionPopUpButtons.Clear();
            search.Show();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void ListBoxTap(object sender, GestureEventArgs e)
        {
            this.NavigationService.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }
    }
}