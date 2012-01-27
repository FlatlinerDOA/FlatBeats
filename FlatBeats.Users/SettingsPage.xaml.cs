namespace FlatBeats.Users
{
    using System;
    using System.Windows.Controls;

    using Clarity.Phone.Controls;

    using FlatBeats.ViewModels;

    public partial class SettingsPage : AnimatedBasePage
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.AnimationContext = this.LayoutRoot;
        }

        private void ListBoxTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.NavigationService.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }
    }
}