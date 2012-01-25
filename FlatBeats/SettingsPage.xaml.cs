namespace FlatBeats
{
    using System;
    using System.Windows.Controls;

    using Clarity.Phone.Controls;

    using Microsoft.Phone.Controls;
    using FlatBeats.ViewModels;

    using GestureEventArgs = System.Windows.Input.GestureEventArgs;

    public partial class SettingsPage : AnimatedBasePage
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.AnimationContext = this.LayoutRoot;
        }

        private void ListBoxTap(object sender, GestureEventArgs e)
        {
            this.NavigationService.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }
    }
}