namespace FlatBeats.Users
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

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
            try
            {
                var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
                this.NavigationService.NavigateTo(navItem);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var box = (PasswordBox)sender;
            BindingExpression be = box.GetBindingExpression(PasswordBox.PasswordProperty);
            be.UpdateSource();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            BindingExpression be = box.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }
    }
}