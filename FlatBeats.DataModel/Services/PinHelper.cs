
namespace FlatBeats.ViewModels
{
    using System;
    using System.Linq;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Shell;

    public static class PinHelper
    {
        public static readonly Uri DefaultTileUrl = new Uri("/", UriKind.Relative);

        public static readonly Uri ApplicationTileBackground = new Uri("appdata:Background.png");

        public static bool IsApplicationPinnedToStart()
        {
            return ShellTile.ActiveTiles.Any(tile => tile.NavigationUri == DefaultTileUrl);
        }

        public static void ResetApplicationTile()
        {
            var appTile = ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri == DefaultTileUrl);
            if (appTile == null)
            {
                return;
            }

            var newAppTile = new StandardTileData()
            {
                BackContent = string.Empty,
                BackBackgroundImage = new Uri(string.Empty, UriKind.Relative),
                BackTitle = string.Empty,
                BackgroundImage = ApplicationTileBackground,
                Title = "Flat Beats"
            };

            appTile.Update(newAppTile);
        }

        public static void UpdateFlipTile(
            string title,
            string backTitle,
            string backContent,
            string wideBackContent,
            int count,
            Uri tileUrl,
            Uri smallBackgroundImage,
            Uri backgroundImage,
            Uri backBackgroundImage,
            Uri wideBackgroundImage,
            Uri wideBackBackgroundImage, 
            bool updateWinPhone7 = true)
        {
            var tileToUpdate = ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri == tileUrl);
            if (tileToUpdate == null)
            {
                return;
            }

            if (PlatformHelper.IsWindowsPhone78OrLater)
            {
                // Get the new FlipTileData type.
                Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");

                // Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

                // Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
                var updateTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                // Set the properties. 
                SetProperty(updateTileData, "Title", title);
                SetProperty(updateTileData, "Count", count);
                SetProperty(updateTileData, "BackTitle", backTitle);
                SetProperty(updateTileData, "BackContent", backContent);
                SetProperty(updateTileData, "SmallBackgroundImage", smallBackgroundImage);
                SetProperty(updateTileData, "BackgroundImage", backgroundImage);
                SetProperty(updateTileData, "BackBackgroundImage", backBackgroundImage);
                SetProperty(updateTileData, "WideBackgroundImage", wideBackgroundImage);
                SetProperty(updateTileData, "WideBackBackgroundImage", wideBackBackgroundImage);
                SetProperty(updateTileData, "WideBackContent", wideBackContent);

                // Invoke the new version of ShellTile.Update.
                shellTileType.GetMethod("Update").Invoke(tileToUpdate, new[] { updateTileData });
            }
            else if (updateWinPhone7)
            {
                var newAppTile = new StandardTileData
                {
                    BackContent = backContent,
                    Count = count,
                    BackTitle = backTitle,
                    BackBackgroundImage = backBackgroundImage,
                    BackgroundImage = backgroundImage,
                    Title = title
                };

                tileToUpdate.Update(newAppTile);
            }
        }

        private static void SetProperty(object instance, string name, object value)
        {
            var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
            setMethod.Invoke(instance, new object[] { value });
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
                var tileUrl = GetPlayPageUrl(mix);
                ShellTile.Create(
                    tileUrl,
                    new StandardTileData
                    {
                        Title = mix.Name,
                        BackContent = mix.Description,
                        BackgroundImage = mix.Cover.ThumbnailUrl,
                        BackTitle = mix.Name
                    });

                UpdateFlipTile(mix.Name, mix.Name, mix.Description, mix.Description, mix.TrackCount, tileUrl, mix.Cover.ThumbnailUrl, mix.Cover.ThumbnailUrl, null, mix.Cover.OriginalUrl, null, false);
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
    }
}
