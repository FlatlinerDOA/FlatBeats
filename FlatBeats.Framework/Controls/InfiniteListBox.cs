namespace FlatBeats.Framework.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Diagnostics;
    using FlatBeats.ViewModels;
    using System.Collections;
    using System.Windows.Media;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    public sealed class InfiniteListBox : ListBox
    {
        public InfiniteListBox()
        {
            this.LoadMoreAtScrollPercentage = 0.75;
            this.Loaded += new RoutedEventHandler(InfiniteListBox_Loaded);
            this.LayoutUpdated += new EventHandler(InfiniteListBox_LayoutUpdated);
            this.Unloaded += new RoutedEventHandler(InfiniteListBox_Unloaded);
        }

        void InfiniteListBox_LayoutUpdated(object sender, EventArgs e)
        {
            this.HookScrollEvents();
        }

        void InfiniteListBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.HookScrollEvents();
        }

        void InfiniteListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= this.InfiniteListBox_Unloaded;
            this.Loaded -= this.InfiniteListBox_Loaded;
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
   
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.HookScrollEvents();
        }
        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void HookScrollEvents()
        {
            if (this.alreadyHookedScrollEvents)
            {
                return;
            }

            this.sv = (ScrollViewer)this.FindElementRecursive(this, typeof(ScrollViewer));
            this.sb = (ScrollBar)this.FindElementRecursive(this, typeof(ScrollBar));
            if (this.sv != null && this.sb != null)
            {
                Debug.WriteLine("Hooked scroll events to " + this.DataContext.ToString());
                this.alreadyHookedScrollEvents = true;

                this.AddHandler(
                    UIElement.ManipulationCompletedEvent,
                    (EventHandler<ManipulationCompletedEventArgs>)this.LB_ManipulationCompleted,
                    true);

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
            var i = this.DataContext as IInfiniteScroll;
            if (i != null)
            {
                Debug.WriteLine("IInfiniteScroll.LoadNextPage()");
                i.LoadNextPage();
            }
        }

        #endregion
    }
}
