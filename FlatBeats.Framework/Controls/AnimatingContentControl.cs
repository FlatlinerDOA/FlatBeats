using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.Framework.Controls
{

    /// <summary>
    /// </summary>
    public class AnimatingContentControl : ContentControl
    {
        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public AnimatingContentControl()
        {
            this.DefaultStyleKey = typeof(AnimatingContentControl);
            this.Loaded += this.AnimatingContentControlLoaded;
            this.Unloaded += this.AnimatingContentControlUnloaded;
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void AnimatingContentControlLoaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "AfterLoaded", true);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void AnimatingContentControlUnloaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "AfterUnLoaded", false);
        }

        #endregion
    }
}
