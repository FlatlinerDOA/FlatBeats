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

        public static Uri Settings()
        {
            return new Uri("/FlatBeatsUsers;component/SettingsPage.xaml", UriKind.Relative);
        }

        public static Uri SearchMixes(string query)
        {
            return new Uri("/MixesPage.xaml?q=" + Uri.EscapeDataString(query), UriKind.Relative);
        }

        public static Uri Play(string mixId, bool autoPlay)
        {
            return new Uri("/PlayPage.xaml?mix=" + mixId + "&play=" + autoPlay.ToString().ToLowerInvariant(), UriKind.Relative);
        }

        public static Uri Tags()
        {
            return new Uri("/TagsPage.xaml", UriKind.Relative);
        }

        public static Uri SearchMixesByTag(string tagName)
        {
            return new Uri("/MixesPage.xaml?tag=" + tagName, UriKind.Relative);
        }
    }
}
