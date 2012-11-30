
namespace FlatBeats.ViewModels
{
    using System;
    using System.Linq;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Shell;

    public static class PinHelper
    {
        public static void UpdateFlipTile(
            string title,
            string backTitle,
            string backContent,
            string wideBackContent,
            int count,
            Uri tileId,
            Uri smallBackgroundImage,
            Uri backgroundImage,
            Uri backBackgroundImage,
            Uri wideBackgroundImage,
            Uri wideBackBackgroundImage)
        {
            if (PlatformHelper.IsWindowsPhone78OrLater)
            {
                // Get the new FlipTileData type.
                Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");

                // Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
                Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

                // Loop through any existing Tiles that are pinned to Start.
                foreach (var tileToUpdate in ShellTile.ActiveTiles)
                {
                    // Look for a match based on the Tile's NavigationUri (tileId).
                    if (tileToUpdate.NavigationUri.ToString() == tileId.ToString())
                    {
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
                        break;
                    }
                }
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

                UpdateFlipTile(mix.Name, mix.Name, mix.Description, mix.Description, mix.TrackCount, tileUrl, mix.Cover.ThumbnailUrl, mix.Cover.ThumbnailUrl, null, mix.Cover.OriginalUrl, null);
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
