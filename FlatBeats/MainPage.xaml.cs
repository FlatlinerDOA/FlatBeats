namespace FlatBeats
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Threading;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using Coding4Fun.Phone.Controls;

    using FlatBeats.Framework.Controls;
    using FlatBeats.Framework;
    using FlatBeats.Framework.Extensions;
    using FlatBeats.ViewModels;

    using Flatliner.Phone;
    using Flatliner.Phone.Controls;

    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Reactive;

    using GestureEventArgs = System.Windows.Input.GestureEventArgs;

    public partial class MainPage : ViewModelPage
    {
        private const string PlayMixKey = "MixId";

        private bool historyItemLaunch;

        private string playMixId;

        // Constructor
        public MainPage()
        {
            this.InitializeComponent();

            // Set the data context of the listbox control to the sample data
            this.Loaded += this.MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            LittleWatson.CheckForPreviousException();
        }

        ////protected override void AnimationsComplete(AnimationType animationType)
        ////{
        ////    base.AnimationsComplete(animationType);

        ////}

        public MainPageViewModel MainPageViewModel
        {
            get
            {
                return (MainPageViewModel)this.DataContext;
            }
        }

        ////protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        ////{
        ////    return null;
        ////}


        ////private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        ////{
        ////    this.Unloaded -= this.MainPage_Unloaded;
        ////}


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

                if (this.historyItemLaunch)
                {
                    this.NavigationService.Navigate(PageUrl.Play(this.playMixId, true));
                }

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
                                this.NavigationService.Navigate(PageUrl.SearchMixes(q.EventArgs.Result));
                            }
                        });
            
            search.ActionPopUpButtons.Clear();
            search.Show();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(PageUrl.Settings());
        }

        private void ListBoxTap(object sender, GestureEventArgs e)
        {
            try
            {
                var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
                this.NavigationService.NavigateTo(navItem);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}