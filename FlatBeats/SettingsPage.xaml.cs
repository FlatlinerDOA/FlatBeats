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
    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.ViewModels;

    using GestureEventArgs = System.Windows.Input.GestureEventArgs;

    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        public SettingsPageViewModel ViewModel 
        { 
            get
            {
                return this.DataContext as SettingsPageViewModel;
            } 
        }    

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Load();
        }

        private void NavigationList_OnNavigation(object sender, NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
        }

        private void ListBoxTap(object sender, GestureEventArgs e)
        {
           this.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }

        private void NavigateTo(INavigationItem navItem)
        {
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }
    }
}