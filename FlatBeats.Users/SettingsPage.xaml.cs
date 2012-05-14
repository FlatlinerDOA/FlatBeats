namespace FlatBeats.Users
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    using Clarity.Phone.Controls;

    using FlatBeats.ViewModels;

    public partial class SettingsPage : AnimatedBasePage
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.AnimationContext = this.LayoutRoot;
            this.trackList.NavigateFunction = navItem => this.NavigationService.NavigateTo(navItem);
        }

        private void ListBoxTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
            this.NavigationService.NavigateTo(navItem);
        }
    }
}