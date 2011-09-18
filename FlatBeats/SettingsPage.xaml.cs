using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace FlatBeats
{
    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.ViewModels;

    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        public SettingsPageViewModel ViewModel 
        { 
            get
            {
                return this.DataContext as SettingsPageViewModel;
            } 
        }    

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplicationBarBinder.Bind(this, this.ViewModel);
            this.ViewModel.Load();
        }
    }
}