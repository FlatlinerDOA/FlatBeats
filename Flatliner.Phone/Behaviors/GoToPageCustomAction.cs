namespace Flatliner.Phone.Behaviors
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;
    using System.Windows.Controls.Primitives;
    using Flatliner.Phone.Extensions;
    using Microsoft.Phone.Controls;

    public class GoToPageCustomAction : System.Windows.Interactivity.TriggerAction<FrameworkElement>
    {
        private Storyboard transition;

        private PhoneApplicationPage page;

        public Uri Page
        {
            get { return (Uri)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Page.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageProperty =
            DependencyProperty.Register("Page", typeof(Uri), typeof(GoToPageCustomAction), new PropertyMetadata(null));


        public string Transition
        {
            get { return (string)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Transition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register("Transition", typeof(string), typeof(GoToPageCustomAction), new PropertyMetadata("LeavePage"));



        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.Loaded += this.AssociatedObjectLoaded;
            }
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            this.page = this.AssociatedObject.FindAncestor<PhoneApplicationPage>();
        }

        protected override void Invoke(object parameter)
        {
            var list = this.AssociatedObject as Selector;
            if (list == null)
            {
                return;
            }

            if (this.page == null)
            { 
                return;
            }

            if (list.SelectedIndex != -1)
            {
                this.transition = (Storyboard)page.FindName(this.Transition);
                if (this.transition != null)
                {
                    this.transition.Completed += this.TransitionCompleted;
                    this.transition.Begin();
                }
            }
        }

        private void TransitionCompleted(object sender, EventArgs e)
        {
            this.NavigateToPage();
        }

        private void NavigateToPage()
        {
            try
            {
                if (this.page == null)
                {
                    return;
                }

                var root = (FrameworkElement)Application.Current.RootVisual;
                root.DataContext = ((Selector)this.AssociatedObject).SelectedItem;
                this.page.NavigationService.Navigate(
                    new Uri("/" + this.Page.OriginalString, UriKind.RelativeOrAbsolute));
            }
            catch (InvalidOperationException)
            {
                // Ignore if app is no longer active.
            }
        }
    }
}
