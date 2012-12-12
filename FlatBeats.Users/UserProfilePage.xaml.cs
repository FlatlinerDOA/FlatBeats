namespace FlatBeats.Users
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Flatliner.Phone.ViewModels;

    public partial class UserProfilePage : AnimatedBasePage
    {
        private ApplicationBarBinder appBarBinder;

        public UserProfilePage()
        {
            this.InitializeComponent();
            this.AnimationContext = this.LayoutRoot;
            this.trackList.NavigateFunction = navItem => this.NavigationService.NavigateTo(navItem);
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (animationType == AnimationType.NavigateForwardIn || animationType == AnimationType.NavigateBackwardOut)
            {
                return this.GetContinuumAnimation(this.FindName("userNameTextBlock") as FrameworkElement, animationType);
            }

            if ((animationType == AnimationType.NavigateForwardOut || animationType == AnimationType.NavigateBackwardIn) && 
                (toOrFrom.IsForPage("PlayPage") || toOrFrom.IsForPage("UserProfilePage")) && 
                this.CurrentListBox != null)
            {
                return this.GetContinuumAnimation(this.CurrentListBox.ItemContainerGenerator.ContainerFromIndex(this.CurrentListBox.SelectedIndex) as FrameworkElement, animationType);
            }

            if (animationType == AnimationType.NavigateBackwardIn)
            {
                return new SlideUpAnimator() { RootElement = LayoutRoot };
            }

            return new SlideDownAnimator() { RootElement = LayoutRoot };
        }

        public ListBox CurrentListBox
        {
            get
            {
                switch (pivot.SelectedIndex)
                {
                    case 1:
                        return this.mixesListBox;
                    case 2:
                        return this.likedMixesListBox;
                    case 3:
                        return this.followsListBox;
                    case 4:
                        return this.followedByListBox;
                }

                return null;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.appBarBinder == null)
            {
                this.appBarBinder = new ApplicationBarBinder(this, (IApplicationBarViewModel)this.ViewModel);
            }

            base.OnNavigatedTo(e);
        }


        private void ListBoxTap(object sender, GestureEventArgs e)
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

        private void ButtonTap(object sender, GestureEventArgs e)
        {
            var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
            this.NavigationService.NavigateTo(navItem);
        }
    }
}