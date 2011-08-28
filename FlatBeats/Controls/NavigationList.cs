// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigationList.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.Controls
{
    using System;
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// A lightweight list control that raises a Navigation event when items are clicked
    /// </summary>
    public class NavigationList : Control
    {
        #region Constants and Fields

        /// <summary>
        ///   Identifies the ItemTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(NavigationList), new PropertyMetadata(null));

        /// <summary>
        ///   Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(NavigationList), new PropertyMetadata(null));

        /// <summary>
        /// </summary>
        private bool _manipulationDeltaStarted;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public NavigationList()
        {
            this.DefaultStyleKey = typeof(NavigationList);
        }

        #endregion

        #region Public Events

        /// <summary>
        ///   Occurs when the user clicks on an item in the list
        /// </summary>
        public event EventHandler<NavigationEventArgs> Navigation;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets / sets the template used to render the list content.
        ///   This is a dependency property.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(ItemTemplateProperty);
            }

            set
            {
                this.SetValue(ItemTemplateProperty, value);
            }
        }

        /// <summary>
        ///   Gets / sets the ItemsSource of the NavigationList. This is a dependency property.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)this.GetValue(ItemsSourceProperty);
            }

            set
            {
                this.SetValue(ItemsSourceProperty, value);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var itemsControl = this.GetTemplateChild("itemsControl") as ItemsControlEx;
            itemsControl.PrepareContainerForItem += this.ItemsControl_PrepareContainerForItem;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the Navigation event
        /// </summary>
        /// <param name="args">
        /// </param>
        protected void OnNavigation(NavigationEventArgs args)
        {
            if (this.Navigation != null)
            {
                this.Navigation(this, args);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void Element_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            this._manipulationDeltaStarted = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void Element_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            this._manipulationDeltaStarted = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this._manipulationDeltaStarted)
            {
                return;
            }

            // raises the Navigation event on mouse up, but only if a manipulation delta
            // has not started.            
            var element = sender as FrameworkElement;
            this.OnNavigation(new NavigationEventArgs(element.DataContext));
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void ItemsControl_PrepareContainerForItem(object sender, PrepareContainerForItemEventArgs e)
        {
            var element = e.Element as UIElement;

            // handle events on the elements added to the ItemsControl
            element.MouseLeftButtonUp += this.Element_MouseLeftButtonUp;
            element.ManipulationStarted += this.Element_ManipulationStarted;
            element.ManipulationDelta += this.Element_ManipulationDelta;
        }

        #endregion
    }

    /// <summary>
    /// Provides data for the Navigation event
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        /// <param name="item">
        /// </param>
        internal NavigationEventArgs(object item)
        {
            this.Item = item;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets the item that was navigated to
        /// </summary>
        public object Item { get; private set; }

        #endregion
    }
}