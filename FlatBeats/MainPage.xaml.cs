namespace FlatBeats
{
    using System;
    using System.Windows;
    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Controls;

    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            this.InitializeComponent();

            // Set the data context of the listbox control to the sample data
            this.DataContext = App.ViewModel;
            this.Loaded += this.MainPage_Loaded;
            this.Unloaded += this.MainPage_Unloaded;
        }

        void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Unload();
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ////App.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;
            this.Dispatcher.BeginInvoke(new Action(App.ViewModel.Load));
            ////this.LoadDynamicBackground();
        }

        ////private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        ////{
        ////    if (e.PropertyName == "BackgroundImage")
        ////    {
        ////        this.LoadDynamicBackground();
        ////    }
        ////}

        ////private void LoadDynamicBackground()
        ////{
        ////    this.pano.DynamicBackground = new ImageBrush
        ////        {
        ////            Stretch = Stretch.UniformToFill,
        ////            Opacity = 0.7,
        ////            ImageSource = new BitmapImage(App.ViewModel.BackgroundImage)
        ////                {
        ////                    CreateOptions = BitmapCreateOptions.BackgroundCreation
        ////                }
        ////        };
        ////}

        private void NavigationList_OnNavigation(object sender, NavigationEventArgs e)
        {
            var navItem = e.Item as INavigationItem;
            if (navItem != null && navItem.NavigationUrl != null)
            {
                this.NavigationService.Navigate(navItem.NavigationUrl);
            }
        }
    }
}