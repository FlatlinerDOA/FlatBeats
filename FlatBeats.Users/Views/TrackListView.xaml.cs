namespace FlatBeats.Users.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using FlatBeats.ViewModels;

    public partial class TrackListView : UserControl
    {
        public TrackListView()
        {
            this.InitializeComponent();
        }

        private void ListBoxTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
            this.NavigateFunction(navItem);
        }

        private void ButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var navItem = ((FrameworkElement)e.OriginalSource).DataContext as INavigationItem;
            this.NavigateFunction(navItem);
        }

        public Action<INavigationItem> NavigateFunction { get; set; }

    }
}
