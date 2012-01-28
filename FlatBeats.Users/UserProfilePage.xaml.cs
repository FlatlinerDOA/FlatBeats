namespace FlatBeats.Users
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using FlatBeats.ViewModels;

    using Flatliner.Phone.ViewModels;

    public partial class UserProfilePage : AnimatedBasePage
    {
        private ApplicationBarBinder appBarBinder;

        public UserProfilePage()
        {
            InitializeComponent();
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
            this.NavigationService.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }
    }
}