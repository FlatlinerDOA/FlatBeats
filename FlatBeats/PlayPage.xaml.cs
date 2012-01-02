﻿namespace FlatBeats
{
    using System;
    using System.Linq;
    using System.Windows.Controls;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    using GestureEventArgs = System.Windows.Input.GestureEventArgs;
    using NavigationEventArgs = System.Windows.Navigation.NavigationEventArgs;

    /// <summary>
    /// </summary>
    public partial class PlayPage : AnimatedBasePage
    {
        private ApplicationBarBinder appBarBinder;

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public PlayPage()
        {
            this.InitializeComponent();
            this.AnimationContext = LayoutRoot;
        }

        #endregion

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            ////if (toOrFrom != null)
            ////{
            ////    if (toOrFrom.OriginalString.Contains("UserProfilePage"))
            ////    {
            ////        return null;
            ////    }
            ////}

            if (animationType == AnimationType.NavigateForwardIn || animationType == AnimationType.NavigateBackwardIn)
            {
                return new SlideUpAnimator() { RootElement = LayoutRoot };
            }

            return new SlideDownAnimator() { RootElement = LayoutRoot };
        }


        #region Public Properties

        /// <summary>
        /// </summary>
        public PlayPageViewModel ViewModel
        {
            get
            {
                return this.DataContext as PlayPageViewModel;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="e">
        /// </param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (this.appBarBinder == null)
            {
                this.appBarBinder = new ApplicationBarBinder(this, this.ViewModel);
            }

            this.ViewModel.MixId = this.NavigationContext.QueryString["mix"];
            this.ViewModel.PlayedPanel.PlayOnLoad = this.NavigationContext.QueryString.ContainsKey("play")
                                        && this.NavigationContext.QueryString["play"] == "true";
            this.ViewModel.Load();
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            this.ViewModel.Unload();
            base.OnNavigatingFrom(e);
        }

        #endregion

        private void NavigationList_OnNavigation(object sender, Controls.NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
            this.NavigationService.NavigateTo(navItem);
        }

        private void UserButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/UserProfilePage.xaml?userid=" + this.ViewModel.CreatedByUserId, UriKind.Relative));
        }

        private void ListBoxTap(object sender, GestureEventArgs e)
        {
            this.NavigationService.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }


        private void ButtonTap(object sender, GestureEventArgs e)
        {
            this.NavigationService.NavigateTo(((Button)sender).DataContext as INavigationItem);
        }
    }
}