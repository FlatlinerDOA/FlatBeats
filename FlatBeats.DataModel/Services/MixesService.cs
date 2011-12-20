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

namespace FlatBeats.DataModel.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;

    public static class MixesService
    {
        private const string LatestMixesCacheFile = "LatestMixes.xml";

        public static IObservable<ReviewsResponseContract> GetMixReviews(string mixId)
        {
            return Downloader.GetJson<ReviewsResponseContract>(new Uri(string.Format("http://8tracks.com/mixes/{0}/reviews.json?per_page=20", mixId), UriKind.RelativeOrAbsolute));
        }
        public static IObservable<MixesResponseContract> GetLatestMixes()
        {
            //&hide_nsfw=1
            return Downloader.GetJson<MixesResponseContract>(
                    new Uri("http://8tracks.com/mixes.json", UriKind.RelativeOrAbsolute), LatestMixesCacheFile);
        }


        public static bool IsPinned(MixContract mix)
        {
            if (mix == null)
            {
                return false;
            }

            return ShellTile.ActiveTiles.Any(t => t.NavigationUri == GetPlayPageUrl(mix));
        }

        private static Uri GetPlayPageUrl(MixContract mix)
        {
            return new Uri("/PlayPage.xaml?mix=" + mix.Id, UriKind.Relative);
        }

        public static void PinToStart(MixContract mix)
        {
            if (!IsPinned(mix))
            {
                ShellTile.Create(
                    GetPlayPageUrl(mix), 
                    new StandardTileData
                    {
                        Title = mix.Name,
                        BackContent = mix.Description,
                        BackgroundImage = mix.Cover.ThumbnailUrl,
                        BackTitle = mix.Name
                    });
            }

        }

        public static void UnpinFromStart(MixContract mix)
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault(t => t.NavigationUri == GetPlayPageUrl(mix));
            if (tile != null)
            {
                tile.Delete();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="tag">
        /// </param>
        /// <param name="sort">
        /// </param>
        /// <returns>
        /// </returns>
        public static IObservable<MixesResponseContract> DownloadTagMixes(string tag, string sort, int pageNumber)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?tag={0}&sort={1}&page={2}", Uri.EscapeDataString(tag), sort, pageNumber),
                    UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// </summary>
        /// <param name="tag">
        /// </param>
        /// <param name="sort">
        /// </param>
        /// <returns>
        /// </returns>
        public static IObservable<MixesResponseContract> DownloadSearchMixes(string query, string sort, int pageNumber)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?q={0}&sort={1}&page={2}", Uri.EscapeDataString(query), sort, pageNumber),
                    UriKind.RelativeOrAbsolute));
        }

        public static IObservable<MixContract> GetMixAsync(string mixId)
        {
            var mixUrl = new Uri(string.Format("http://8tracks.com/mixes/{0}.json", mixId), UriKind.RelativeOrAbsolute);
            return from response in Downloader.GetJson<MixResponseContract>(mixUrl)
                   select response.Mix;
        }
    }
}
