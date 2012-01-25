namespace FlatBeats
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Threading;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;
    using Clarity.Phone.Extensions;

    using Coding4Fun.Phone.Controls;

    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    using Flatliner.Phone;

    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Reactive;

    using GestureEventArgs = System.Windows.Input.GestureEventArgs;
    using NavigationEventArgs = FlatBeats.Controls.NavigationEventArgs;

    public partial class MainPage : AnimatedBasePage
    {
        private const string PlayMixKey = "MixId";

        private bool historyItemLaunch;

        private string playMixId;

        // Constructor
        public MainPage()
        {
            this.InitializeComponent();

            this.AnimationContext = this.LayoutRoot;

            // Set the data context of the listbox control to the sample data
            this.Loaded += this.MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            LittleWatson.CheckForPreviousException();
        }

        protected override void AnimationsComplete(AnimationType animationType)
        {
            base.AnimationsComplete(animationType);

            if (historyItemLaunch)
            {
                this.NavigationService.Navigate(new Uri("/PlayPage.xaml?mix=" + this.playMixId + "&play=true", UriKind.Relative));
            }
        }

        public MainPageViewModel MainPageViewModel
        {
            get
            {
                return (MainPageViewModel)this.DataContext;
            }
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            return null;
        }


        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= this.MainPage_Unloaded;
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.historyItemLaunch = false;
            this.playMixId = null;
            if (e.NavigationMode == NavigationMode.New && NavigationContext.QueryString.ContainsKey(PlayMixKey))
            {
                // We were launched from a history item.
                // Change _playingSong even if something was already playing 
                // because the user directly chose a song history item.

                // Use the navigation context to find the song by name.
                this.playMixId = NavigationContext.QueryString[PlayMixKey];

                // Set a flag to indicate that we were started from a 
                // history item and that we should immediately start 
                // playing the song once the UI has finished loading.
                this.historyItemLaunch = true;
                return;
            }

            base.OnNavigatedTo(e);
        }

        private void pano_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pano.SelectedIndex != 2 || this.ViewModel.IsInProgress)
            {
                HubTileService.FreezeGroup("Latest");
            }
            else
            {
                HubTileService.UnfreezeGroup("Latest");
            }

            this.MainPageViewModel.CurrentSectionIndex = pano.SelectedIndex;
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