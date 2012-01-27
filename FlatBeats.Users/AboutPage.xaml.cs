namespace FlatBeats.Users
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Phone.Controls;
    using FlatBeats.ViewModels;

    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            this.InitializeComponent();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (HyperlinkButton)sender;
            var uri = new Uri(button.Tag.ToString(), UriKind.Absolute);
            this.NavigationService.NavigateTo(uri);
        }
    }
}