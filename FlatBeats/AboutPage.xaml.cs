using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace FlatBeats
{
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Tasks;

    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (HyperlinkButton)sender;
            var uri = new Uri(button.Tag.ToString(), UriKind.Absolute);
            this.NavigationService.NavigateTo(uri);
        }
    }
}