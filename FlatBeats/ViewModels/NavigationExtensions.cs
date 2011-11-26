using System;
using System.Net;

namespace FlatBeats.ViewModels
{
    using System.Windows.Navigation;

    using Microsoft.Phone.Tasks;

    public static class NavigationExtensions
    {
        public static void NavigateTo(this NavigationService navService, INavigationItem navItem)
        {
            if (navItem != null && navItem.NavigationUrl != null)
            {
                if (navItem.NavigationUrl.IsAbsoluteUri)
                {
                    if (navItem.NavigationUrl.Scheme == Uri.UriSchemeHttp)
                    {
                        WebBrowserTask task = new WebBrowserTask();
                        task.Uri = navItem.NavigationUrl;
                        task.Show();
                    }
                    else if (navItem.NavigationUrl.Scheme == "music")
                    {
                        MarketplaceSearchTask task = new MarketplaceSearchTask();
                        task.ContentType = MarketplaceContentType.Music;
                        task.SearchTerms = HttpUtility.UrlDecode(navItem.NavigationUrl.Query.TrimStart('?'));
                        task.Show();
                    }
                }
                else
                {
                    navService.Navigate(navItem.NavigationUrl);
                }
            }
        }
    }
}
