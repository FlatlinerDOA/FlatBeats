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

    public partial class TagsPage : PhoneApplicationPage
    {
        public TagsPage()
        {
            InitializeComponent();
        }

        public TagsPageViewModel ViewModel
        {
            get
            {
                return (TagsPageViewModel)this.DataContext;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.Dispatcher.BeginInvoke(new Action(() => this.ViewModel.Load()));
        }

        private void NavigationList_OnNavigation(object sender, Controls.NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }

        private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var navItem = tagsList.SelectedItem as INavigationItem;
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }
    }
}