namespace FlatBeats
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Clarity.Phone.Controls;
    using Clarity.Phone.Controls.Animations;

    using FlatBeats.ViewModels;

    public partial class MixesPage : AnimatedBasePage
    {
        public MixesPage()
        {
            this.InitializeComponent();
            this.AnimationContext = LayoutRoot;
        }

        protected override AnimatorHelperBase GetAnimation(AnimationType animationType, Uri toOrFrom)
        {
            if (animationType == AnimationType.NavigateForwardOut || animationType == AnimationType.NavigateBackwardIn)
            {
                return GetContinuumAnimation(this.CurrentListBox.ItemContainerGenerator.ContainerFromIndex(this.CurrentListBox.SelectedIndex) as FrameworkElement, animationType);
            }

            return base.GetAnimation(animationType, toOrFrom);
        }

        public MixesPageViewModel MixesViewModel 
        { 
            get
            {
                return (MixesPageViewModel)this.DataContext;
            } 
        }

        public ListBox CurrentListBox
        {
            get
            {
                switch (pivot.SelectedIndex)
                {
                    case 0:
                        return this.recentList;
                    case 1:
                        return this.hotList;
                    case 2:
                        return this.popularList;
                }

                return this.recentList;
            }
        }


        private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MixesViewModel.CurrentPanelIndex = pivot.SelectedIndex;
        }

        private void ListBoxTapped(object sender, GestureEventArgs e)
        {
            this.NavigationService.NavigateTo(((ListBox)sender).SelectedItem as INavigationItem);
        }
    }
}