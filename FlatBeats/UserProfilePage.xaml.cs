﻿using System;
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
    using FlatBeats.ViewModels;

    public partial class UserProfilePage : PhoneApplicationPage
    {
        private ApplicationBarBinder appBarBinder;

        public UserProfilePage()
        {
            this.InitializeComponent();
        }

        public UserProfilePageViewModel ViewModel 
        { 
            get
            {
                return (UserProfilePageViewModel)this.DataContext;
            } 
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (this.appBarBinder == null)
            {
                this.appBarBinder = new ApplicationBarBinder(this, this.ViewModel);
            }

            this.ViewModel.UserId = this.NavigationContext.QueryString["userid"];
            this.ViewModel.Load();
        }


        private void NavigationList_OnNavigation(object sender, Controls.NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }
    }
}