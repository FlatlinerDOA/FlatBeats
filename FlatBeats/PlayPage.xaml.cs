namespace FlatBeats
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using FlatBeats.Framework.Controls;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Flatliner.Phone.ViewModels;

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
            if ((animationType == AnimationType.NavigateForwardOut || animationType == AnimationType.NavigateBackwardIn) && toOrFrom.IsForPage("UserProfilePage"))
            {
                if (this.pivot.SelectedIndex == 0)
                {
                    return this.GetContinuumAnimation(this.FindName("createdByTextBlock") as FrameworkElement, animationType);
                } 
                else
                {
                    return this.GetContinuumAnimation(this.reviewsListBox.ItemContainerGenerator.ContainerFromIndex(this.reviewsListBox.SelectedIndex) as FrameworkElement, animationType);
                }
            }

            if (animationType == AnimationType.NavigateForwardIn || animationType == AnimationType.NavigateBackwardOut)
            {
                return this.GetContinuumAnimation(this.FindName("mixNameTextBlock") as FrameworkElement, animationType);
            }

            if (animationType == AnimationType.NavigateBackwardIn)
            {
                return new SlideUpAnimator() { RootElement = LayoutRoot };
            }

            return new SlideDownAnimator() { RootElement = LayoutRoot };
        }


        #region Public Properties

        /// <summary>
        /// </summary>
        public PlayPageViewModel PageViewModel
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
            if (this.appBarBinder == null)
            {
                this.appBarBinder = new ApplicationBarBinder(this, this.PageViewModel);
            }

            base.OnNavigatedTo(e);
        }

        public void HideAppBar()
        {
            this.ApplicationBar.IsVisible = false;
        }
        
        public void ShowAppBar()
        {
            this.ApplicationBar.IsVisible = true;
        }
        #endregion

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this.pivot.Opacity == 0)
            {
                e.Cancel = true;
                ((Storyboard)this.Resources["ShowPivotStoryboard"]).Begin();
                this.ShowAppBar();
                return;
            }
            
            if (((PlayPageViewModel)this.ViewModel).IsPromptOpen)
            {
                e.Cancel = true;
                return;
            }

            base.OnBackKeyPress(e);
        }
     

        private void UserButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(PageUrl.UserProfile(this.PageViewModel.CreatedByUserId, this.PageViewModel.CreatedByUserName));
        }

        private void ListBoxTap(object sender, GestureEventArgs e)
        {
            var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
            this.NavigationService.NavigateTo(navItem);
        }


        private void ButtonTap(object sender, GestureEventArgs e)
        {
            var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
            this.NavigationService.NavigateTo(navItem);
        }
    }
}