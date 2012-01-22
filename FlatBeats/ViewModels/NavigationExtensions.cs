using System;
using System.Net;

namespace FlatBeats.ViewModels
{
    using System.Windows.Navigation;

    using Microsoft.Phone.Tasks;

    public static class Avatar
    {
        public static Uri ParseUrl(string imageUrl)
        {
            Uri url;
            if (!Uri.TryCreate(imageUrl, UriKind.RelativeOrAbsolute, out url))
            {
                return new Uri("/ApplicationIcon.png", UriKind.Relative);
            }

            if (!url.IsAbsoluteUri)
            {
                url = new Uri("http://8tracks.com" + imageUrl, UriKind.Absolute);
            }

            return url;
        }
    }

    public static class NavigationExtensions
    {
        public static void NavigateTo(this NavigationService navService, INavigationItem navItem)
        {
            if (navItem == null)
            {
                return;
            }

            navService.NavigateTo(navItem.NavigationUrl);
        }

        public static void NavigateTo(this NavigationService navService, Uri url)
        {
            if (url == null)
            {
                return;
            }

            if (url.IsAbsoluteUri)
            {
                try
                {
                    if (url.Scheme == Uri.UriSchemeHttp)
                    {
                        WebBrowserTask task = new WebBrowserTask();
                        task.Uri = url;
                        task.Show();
                    }
                    else if (url.Scheme == Uri.UriSchemeMailto)
                    {
                        var email = url.OriginalString.Substring(Uri.UriSchemeMailto.Length);
                        var emailComposeTask = new EmailComposeTask { To = email, Subject = "Feedback on Flat Beats" };
                        emailComposeTask.Show();
                    }
                    else if (url.Scheme == "music")
                    {
                        MarketplaceSearchTask task = new MarketplaceSearchTask();
                        task.ContentType = MarketplaceContentType.Music;
                        task.SearchTerms = HttpUtility.UrlDecode(url.Query.TrimStart('?'));
                        task.Show();
                    }
                    else if (url.Scheme == "rate")
                    {
                        MarketplaceReviewTask reviewTask = new MarketplaceReviewTask();
                        reviewTask.Show();
                    }
                }
                catch (InvalidOperationException)
                {
                    // Thrown when task is not in the foreground.
                }
            }
            else
            {
                navService.Navigate(url);
            }
        }
    }
}
