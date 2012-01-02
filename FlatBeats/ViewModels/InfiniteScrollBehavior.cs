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

namespace FlatBeats.ViewModels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Controls.Primitives;
    using System.Windows.Interactivity;

    public class InfiniteScrollBehavior : Behavior<ListBox>
    {
        private bool alreadyHookedScrollEvents;

        private ScrollBar sb;

        private ScrollViewer sv;


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += this.AssociatedObjectLoaded;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= this.AssociatedObjectLoaded;
            base.OnDetaching();
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
         if(alreadyHookedScrollEvents) 
             return; 

         alreadyHookedScrollEvents = true; 
         AssociatedObject.AddHandler(UIElement.ManipulationCompletedEvent, (EventHandler<ManipulationCompletedEventArgs>)LB_ManipulationCompleted, true); 
         sb = (ScrollBar)FindElementRecursive(AssociatedObject, typeof(ScrollBar)); 
         sv = (ScrollViewer)FindElementRecursive(AssociatedObject, typeof(ScrollViewer)); 

         if(sv != null) 
         { 
             // Visual States are always on the first child of the control template 
            FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement; 
             if(element != null) 
             { 
                 VisualStateGroup group = FindVisualState(element, "ScrollStates"); 
                 if(group != null) 
                 { 
                     group.CurrentStateChanging += this.group_CurrentStateChanging; 
                 } 
                 VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression"); 
                 VisualStateGroup hgroup = FindVisualState(element, "HorizontalCompression"); 
                 if(vgroup != null) 
                 { 
                     vgroup.CurrentStateChanging += this.vgroup_CurrentStateChanging; 
                 } 
                 if(hgroup != null) 
                 { 
                     hgroup.CurrentStateChanging += this.hgroup_CurrentStateChanging; 
                 } 
             } 
         }           

     }

        private void hgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            
        }

        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionBottom")
            {
                Debug.WriteLine("LOAD MORE!");
                var i = this.AssociatedObject.DataContext as IInfiniteScroll;
                if (i != null)
                {
                    i.LoadNextPage();
                }
            }
            
        }

        private void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
        }

        private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            
        }

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
                       returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType); 
                   } 
               } 
           } 
           return returnElement; 
       }


       private VisualStateGroup FindVisualState(FrameworkElement element, string name) 
       { 
           if (element == null) 
               return null; 

           IList groups = VisualStateManager.GetVisualStateGroups(element);
           return groups.Cast<VisualStateGroup>().FirstOrDefault(group => group.Name == name);
       } 

    }
}
