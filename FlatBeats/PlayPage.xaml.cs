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

namespace EightTracks
{
    using EightTracks.ViewModels;

    public partial class PlayPage : PhoneApplicationPage
    {
        public PlayPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Load();
        }

        public PlayPageViewModel ViewModel 
        { 
            get
            {
                return this.DataContext as PlayPageViewModel;
            } 
        }
    }
}