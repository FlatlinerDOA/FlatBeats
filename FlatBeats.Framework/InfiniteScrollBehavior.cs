// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InfiniteScrollBehavior.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System;

namespace FlatBeats.ViewModels
{
    using System.Collections;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using System.Windows.Media;

    /// <summary>
    /// </summary>
    public class InfiniteScrollBehavior : Behavior<ListBox>
    {
        public InfiniteScrollBehavior()
        {
            this.LoadMoreAtScrollPercentage = 0.75;
        }

        #region Constants and Fields

        /// <summary>
        /// </summary>
        private bool alreadyHookedScrollEvents;

        /// <summary>
        /// </summary>
        private ScrollBar sb;

        /// <summary>
        /// </summary>
        private ScrollViewer sv;

        public double LoadMoreAtScrollPercentage { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += this.AssociatedObjectLoaded;
        }

        /// <summary>
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= this.AssociatedObjectLoaded;
            base.OnDetaching();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            if (this.alreadyHookedScrollEvents)
            {
                return;
            }

            this.alreadyHookedScrollEvents = true;
            this.AssociatedObject.AddHandler(
                UIElement.ManipulationCompletedEvent, 
                (EventHandler<ManipulationCompletedEventArgs>)this.LB_ManipulationCompleted, 
                true);
            this.sb = (ScrollBar)this.FindElementRecursive(this.AssociatedObject, typeof(ScrollBar));
            this.sv = (ScrollViewer)this.FindElementRecursive(this.AssociatedObject, typeof(ScrollViewer));
            if (this.sv != null)
            {

                this.sb.ValueChanged += new RoutedPropertyChangedEventHandler<double>(sb_ValueChanged);
                
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(this.sv, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup group = this.FindVisualState(element, "ScrollStates");
                    if (group != null)
                    {
                        group.CurrentStateChanging += this.group_CurrentStateChanging;
                    }

                    VisualStateGroup vgroup = this.FindVisualState(element, "VerticalCompression");
                    VisualStateGroup hgroup = this.FindVisualState(element, "HorizontalCompression");
                    if (vgroup != null)
                    {
                        vgroup.CurrentStateChanging += this.vgroup_CurrentStateChanging;
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot find VerticalCompression VisualStateGroup, Infinite Scroll will not work!");
                    }

                    if (hgroup != null)
                    {
                        hgroup.CurrentStateChanging += this.hgroup_CurrentStateChanging;
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot find HorizontalCompression VisualStateGroup, Infinite Scroll will not work!");
                    }
                }
            }
        }

        void sb_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((e.NewValue / sb.Maximum) > this.LoadMoreAtScrollPercentage)
            {
                Debug.WriteLine("Scrolled: " + (e.NewValue / sb.Maximum) + " past " + this.LoadMoreAtScrollPercentage);
                this.LoadNext();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="parent">
        /// </param>
        /// <param name="targetType">
        /// </param>
        /// <returns>
        /// </returns>
        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        return element as UIElement;
                    }
                    else
                    {
                        returnElement =
                            this.FindElementRecursive(
                                VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType);
                    }
                }
            }

            return returnElement;
        }

        /// <summary>
        /// </summary>
        /// <param name="element">
        /// </param>
        /// <param name="name">
        /// </param>
        /// <returns>
        /// </returns>
        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
            {
                return null;
            }

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            return groups.Cast<VisualStateGroup>().FirstOrDefault(group => group.Name == name);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void hgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionBottom")
            {
                this.LoadNext();
            }
        }

        private void LoadNext()
        {
            Debug.WriteLine("LOAD MORE!");
            var i = this.AssociatedObject.DataContext as IInfiniteScroll;
            if (i != null)
            {
                i.LoadNextPage();
            }
        }

        #endregion
    }
}