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

namespace FlatBeats
{
    using System.Windows.Navigation;

    using FlatBeats.Framework;

    using Microsoft.Phone.Reactive;

    public partial class SearchPanel : UserControl
    {
        private Subject<string> searches;

        public Subject<string> Searches 
        { 
            get
            {
                return this.searches = (this.searches ?? new Subject<string>());
            } 
        }

        public SearchPanel()
        {
            this.InitializeComponent();
        }

        private NavigationService navigation;

        public NavigationService Navigation
        {
            get
            {
                return this.navigation;
            }
            set
            {
                if (this.navigation == value)
                {
                    return;
                }

                if (this.navigation != null)
                {
                    this.navigation.Navigating -= this.Navigating;
                }

                this.navigation = value;

                if (this.navigation != null)
                {
                    this.navigation.Navigating += this.Navigating;
                }
            }
        }

        private void Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
                this.CloseSearch();
            }
        }

        private void PhoneTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            this.PerformSearch();
        }

        private void PhoneTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                this.PerformSearch();
            }
        }

        private void CloseSearch()
        {
            if (this.navigation != null)
            {
                this.navigation.Navigating -= this.Navigating;
            }

            if (this.searches != null)
            {
                this.searches.OnCompleted();
                this.searches = null;
            }
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(this.searchText.Text)) 
            { 
                this.CloseSearch();
                return;
            }

            this.Searches.OnNext(this.searchText.Text);
            try
            {
                this.Navigation.Navigate(PageUrl.SearchMixes(this.searchText.Text));
            }
            catch (InvalidOperationException)
            {
                // Thrown when task is not in the foreground.
            }
        }
    }
}
