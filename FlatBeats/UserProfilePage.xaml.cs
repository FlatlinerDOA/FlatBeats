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
    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    using GestureEventArgs = System.Windows.Input.GestureEventArgs;

    public partial class UserProfilePage : AnimatedBasePage
    {
        private ApplicationBarBinder appBarBinder;

        public UserProfilePage()
        {
            this.InitializeComponent();
            this.AnimationContext = this.LayoutRoot;
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            ////if (toOrFrom != null)
            ////{
            ////    if (toOrFrom.OriginalString.Contains("MixesPage.xaml"))
            ////    {
            ////        if (animationType == AnimationType.NavigateForwardIn)
            ////        {
            ////            return new TurnstileFeatherForwardInAnimator() { RootElement = LayoutRoot, ListBox = this.mixesListBox };
            ////        }

            ////        return new TurnstileFeatherBackwardInAnimator() { RootElement = LayoutRoot, ListBox = this.mixesListBox };

            ////    }
            ////}

            if (animationType == AnimationType.NavigateForwardIn || animationType == AnimationType.NavigateBackwardIn)
            {
                return new SlideUpAnimator() { RootElement = LayoutRoot };
            }

            return new SlideDownAnimator() { RootElement = LayoutRoot };
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