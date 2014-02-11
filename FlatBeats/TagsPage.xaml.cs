namespace FlatBeats
{
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
    using System.Windows.Navigation;

    using Clarity.Phone.Controls;

    using FlatBeats.ViewModels;

    public partial class TagsPage : AnimatedBasePage
    {
        private LongListSelector currentSelector;

        public TagsPage()
        {
            this.InitializeComponent();
            this.AnimationContext = this.LayoutRoot;
        }

        ////protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        ////{
        ////    if (isGroupOpen)
        ////    {
        ////        e.Cancel = true;
        ////        return;
        ////    }

        ////    base.OnBackKeyPress(e);
        ////}

        ////private bool isGroupOpen;

        ////private void LongListSelector_GroupViewOpened(object sender, LongListSelectorGroupViewOpenedEventArgs e)
        ////{
        ////    isGroupOpen = true;
        ////    //Hold a reference to the active long list selector.
        ////    currentSelector = sender as LongListSelector;

        ////    //Construct and begin a swivel animation to pop in the group view.
        ////    IEasingFunction quadraticEase = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        ////    Storyboard swivelShow = new Storyboard();
        ////    ItemsControl groupItems = e.ItemsControl;

        ////    foreach (var item in groupItems.Items)
        ////    {
        ////        UIElement container = groupItems.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
        ////        if (container != null)
        ////        {
        ////            Border content = VisualTreeHelper.GetChild(container, 0) as Border;
        ////            if (content != null)
        ////            {
        ////                DoubleAnimationUsingKeyFrames showAnimation = new DoubleAnimationUsingKeyFrames();

        ////                EasingDoubleKeyFrame showKeyFrame1 = new EasingDoubleKeyFrame();
        ////                showKeyFrame1.KeyTime = TimeSpan.FromMilliseconds(0);
        ////                showKeyFrame1.Value = -90;
        ////                showKeyFrame1.EasingFunction = quadraticEase;

        ////                EasingDoubleKeyFrame showKeyFrame2 = new EasingDoubleKeyFrame();
        ////                showKeyFrame2.KeyTime = TimeSpan.FromMilliseconds(250);
        ////                showKeyFrame2.Value = 0;
        ////                showKeyFrame2.EasingFunction = quadraticEase;

        ////                showAnimation.KeyFrames.Add(showKeyFrame1);
        ////                showAnimation.KeyFrames.Add(showKeyFrame2);

        ////                Storyboard.SetTargetProperty(showAnimation, new PropertyPath(PlaneProjection.RotationXProperty));
        ////                Storyboard.SetTarget(showAnimation, content.Projection);

        ////                swivelShow.Children.Add(showAnimation);
        ////            }
        ////        }
        ////    }

        ////    swivelShow.Begin();
        ////}

        ////private void LongListSelector_GroupViewClosing(object sender, GroupViewClosingEventArgs e)
        ////{
        ////    // Cancelling automatic closing and scrolling to do it manually.
        ////    e.Cancel = true;
        ////    if (e.SelectedGroup != null)
        ////    {
        ////        currentSelector.ScrollToGroup(e.SelectedGroup);
        ////    }

        ////    //Dispatch the swivel animation for performance on the UI thread.
        ////    Dispatcher.BeginInvoke(() =>
        ////    {
        ////        //Construct and begin a swivel animation to pop out the group view.
        ////        IEasingFunction quadraticEase = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        ////        Storyboard swivelHide = new Storyboard();
        ////        ItemsControl groupItems = e.ItemsControl;

        ////        foreach (var item in groupItems.Items)
        ////        {
        ////            UIElement container = groupItems.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
        ////            if (container != null)
        ////            {
        ////                Border content = VisualTreeHelper.GetChild(container, 0) as Border;
        ////                if (content != null)
        ////                {
        ////                    DoubleAnimationUsingKeyFrames showAnimation = new DoubleAnimationUsingKeyFrames();

        ////                    EasingDoubleKeyFrame showKeyFrame1 = new EasingDoubleKeyFrame();
        ////                    showKeyFrame1.KeyTime = TimeSpan.FromMilliseconds(0);
        ////                    showKeyFrame1.Value = 0;
        ////                    showKeyFrame1.EasingFunction = quadraticEase;

        ////                    EasingDoubleKeyFrame showKeyFrame2 = new EasingDoubleKeyFrame();
        ////                    showKeyFrame2.KeyTime = TimeSpan.FromMilliseconds(125);
        ////                    showKeyFrame2.Value = 90;
        ////                    showKeyFrame2.EasingFunction = quadraticEase;

        ////                    showAnimation.KeyFrames.Add(showKeyFrame1);
        ////                    showAnimation.KeyFrames.Add(showKeyFrame2);

        ////                    Storyboard.SetTargetProperty(showAnimation, new PropertyPath(PlaneProjection.RotationXProperty));
        ////                    Storyboard.SetTarget(showAnimation, content.Projection);

        ////                    swivelHide.Children.Add(showAnimation);
        ////                }
        ////            }
        ////        }

        ////        swivelHide.Completed += this.SwivelHideCompleted;
        ////        swivelHide.Begin();

        ////    });          
        ////}

        ////private void SwivelHideCompleted(object sender, EventArgs e)
        ////{
        ////    // Close group view.
        ////    if (this.currentSelector != null)
        ////    {
        ////        this.currentSelector.CloseGroupView();
        ////        this.currentSelector = null;
        ////    }

        ////    isGroupOpen = false;
        ////}

        private void ListSelectorTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
            this.NavigationService.NavigateTo(navItem);
        }

        private void TagsList_OnJumpListOpening(object sender, EventArgs e)
        {
            
        }

        private void TagsList_OnJumpListClosed(object sender, EventArgs e)
        {
            
        }
    }
}