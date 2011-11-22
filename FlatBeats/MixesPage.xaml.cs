namespace FlatBeats
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using FlatBeats.ViewModels;

    using NavigationEventArgs = FlatBeats.Controls.NavigationEventArgs;

    public partial class MixesPage : AnimatedBasePage
    {
        public MixesPage()
        {
            this.InitializeComponent();
            this.AnimationContext = LayoutRoot;
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (animationType == AnimationType.NavigateForwardOut || animationType == AnimationType.NavigateBackwardIn)
            {
                return GetContinuumAnimation(this.CurrentListBox.ItemContainerGenerator.ContainerFromIndex(this.CurrentListBox.SelectedIndex) as FrameworkElement, animationType);
            }

            return base.GetAnimation(animationType, toOrFrom);
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

        public ListBox CurrentListBox
        {
            get
            {
                switch (pivot.SelectedIndex)
                {
                    case 0:
                        return this.recentList;
                    case 1:
                        return this.hotList;
                    case 2:
                        return this.popularList;
                }

                return this.recentList;
            }
        }

        private void NavigationList_OnNavigation(object sender, NavigationEventArgs e)
        {
            NavigateTo(e.Item as INavigationItem);
        }

        private void NavigateTo(INavigationItem navItem)
        {
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }

        private void PivotSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.ViewModel.CurrentPanelIndex = pivot.SelectedIndex;
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }

        private void ListBoxTapped(object sender, GestureEventArgs e)
        {
            this.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }
    }
}