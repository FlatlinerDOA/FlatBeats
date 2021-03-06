﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace FlatBeats.Framework.Controls
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using Flatliner.Phone.Extensions;

    using Microsoft.Phone.Controls;

    /// <summary>
    /// A Panorama control whose background fades into the new background when
    /// set to a new value.
    /// </summary>
    public class DynamicBackgroundPanorama : Panorama
    {
        /// <summary>
        /// Initializes a new instance of the DynamicBackgroundPanorama type.
        /// </summary>
        public DynamicBackgroundPanorama() : base()
        {
            this.DefaultStyleKey = typeof(DynamicBackgroundPanorama);
        }

        ////public override void OnApplyTemplate()
        ////{
        ////    base.OnApplyTemplate();
        ////    this.transition = this.GetTemplateChild("PART_Transition") as TransitioningBackgroundControl;
        ////    if (this.transition != null)
        ////    {
        ////        this.transition.TransitionCompleted += this.TransitionTransitionCompleted;
        ////    }
        ////}

        ////private void TransitionTransitionCompleted(object sender, RoutedEventArgs e)
        ////{
        ////    this.Background = this.transition.DynamicBackground;
        ////}


        public Brush DynamicBackground
        {
            get { return (Brush)GetValue(DynamicBackgroundProperty); }
            set { SetValue(DynamicBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DynamicBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DynamicBackgroundProperty =
            DependencyProperty.Register("DynamicBackground", typeof(Brush), typeof(DynamicBackgroundPanorama), new PropertyMetadata(null, OnDynamicBackgroundChanged));

        public static void OnDynamicBackgroundChanged(DependencyObject s, DependencyPropertyChangedEventArgs c)
        {
            ////var dp = (DynamicBackgroundPanorama)s;
            ////dp.SetNewBackground((Brush)c.NewValue);
        }

        ////public void SetNewBackground(Brush brush)
        ////{
        ////    if (this.transition != null)
        ////    {
        ////        this.transition.DynamicBackground = brush;
        ////    }   
        ////}
    }
}
