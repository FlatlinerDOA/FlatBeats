// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemsControlEx.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Extends an ItemsControl, raising an event when the PrepareContainerForItemOverride
    ///   override is invoked.
    /// </summary>
    public class ItemsControlEx : ItemsControl
    {
        #region Public Events

        /// <summary>
        ///   Occurs when the PrepareContainerForItemOverride method is invoked
        /// </summary>
        public event EventHandler<PrepareContainerForItemEventArgs> PrepareContainerForItem;

        #endregion

        #region Methods

        /// <summary>
        /// Raises the PrepareContainerForItem event.
        /// </summary>
        /// <param name="args">
        /// </param>
        protected void OnPrepareContainerForItem(PrepareContainerForItemEventArgs args)
        {
            if (this.PrepareContainerForItem != null)
            {
                this.PrepareContainerForItem(this, args);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="element">
        /// </param>
        /// <param name="item">
        /// </param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            this.OnPrepareContainerForItem(new PrepareContainerForItemEventArgs(element, item));
        }

        #endregion
    }

    /// <summary>
    /// Provides data for the PrepareContainerForItem event.
    /// </summary>
    public class PrepareContainerForItemEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        /// <param name="element">
        /// </param>
        /// <param name="item">
        /// </param>
        public PrepareContainerForItemEventArgs(DependencyObject element, object item)
        {
            this.Element = element;
            this.Item = item;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public DependencyObject Element { get; private set; }

        /// <summary>
        /// </summary>
        public object Item { get; private set; }

        #endregion
    }
}