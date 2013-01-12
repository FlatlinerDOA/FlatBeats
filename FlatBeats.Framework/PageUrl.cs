namespace FlatBeats.Framework
{
    using System;

    public static class PageUrl
    {
        public static Uri UserProfile(string userId)
        {
            return new Uri("/FlatBeatsUsers;component/UserProfilePage.xaml?userid=" + userId, UriKind.Relative);
        }

        public static Uri UserProfile(string userId, string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return UserProfile(userId);
            }

            return new Uri("/FlatBeatsUsers;component/UserProfilePage.xaml?userid=" + userId + "&username=" + Uri.EscapeDataString(userName), UriKind.Relative);
        }


        public static Uri Settings()
        {
            return new Uri("/FlatBeatsUsers;component/SettingsPage.xaml", UriKind.Relative);
        }

        public static Uri SearchMixes(string query)
        {
            return new Uri("/MixesPage.xaml?q=" + Uri.EscapeDataString(query), UriKind.Relative);
        }

        public static Uri SearchMixesByTag(string tagName)
        {
            return new Uri("/MixesPage.xaml?tag=" + tagName, UriKind.Relative);
        }

        public static Uri Play(string mixId, bool autoPlay)
        {
            return new Uri("/PlayPage.xaml?mix=" + mixId + "&play=" + autoPlay.ToString().ToLowerInvariant(), UriKind.Relative);
        }

        public static Uri Play(string mixId, bool autoPlay, string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return Play(mixId, autoPlay);
            }

            return new Uri("/PlayPage.xaml?mix=" + mixId + "&play=" + autoPlay.ToString().ToLowerInvariant() + "&title=" + Uri.EscapeDataString(title), UriKind.Relative);
        }


        public static Uri Tags()
        {
            return new Uri("/TagsPage.xaml", UriKind.Relative);
        }

        public static bool IsForPage(this Uri url, string pageName)
        {
            if (url == null || pageName == null)
            {
                return false;
            }

            return url.OriginalString.IndexOf("/" + pageName + ".xaml", StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}
