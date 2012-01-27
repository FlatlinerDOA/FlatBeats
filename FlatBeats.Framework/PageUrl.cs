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

namespace FlatBeats.Framework
{
    public static class PageUrl
    {
        public static Uri UserProfile(string userId)
        {
            return new Uri("/FlatBeatsUsers;component/UserProfilePage.xaml?userid=" + userId, UriKind.Relative);
        }

        public static Uri Settings
        {
            get
            {
                return new Uri("/FlatBeatsUsers;component/SettingsPage.xaml", UriKind.Relative);
            }
        }
    }
}
