namespace FlatBeats
{
    using System;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using FlatBeats.Controls;
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
            if (this.appBarBinder == null)
            {
                this.appBarBinder = new ApplicationBarBinder(this, this.ViewModel);
            }

            base.OnNavigatedTo(e);
        }


        #endregion

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this.pivot.Opacity == 0)
            {
                e.Cancel = true;
                ((Storyboard)this.Resources["ShowPivotStoryboard"]).Begin();
                return;
            }

            base.OnBackKeyPress(e);
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