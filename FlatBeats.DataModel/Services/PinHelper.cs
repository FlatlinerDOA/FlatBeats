
namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Linq;
    using System.Reflection;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Shell;

    public static class PinHelper
    {
        private static readonly Uri ResetUrl = new Uri(string.Empty, UriKind.Relative);
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
                BackBackgroundImage = ResetUrl,
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
            Uri wideBackBackgroundImage)
        {
            var tileToUpdate = ShellTile.ActiveTiles.FirstOrDefault(tile => tile.NavigationUri.ToString() == tileUrl.ToString());
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
                if (flipTileDataType == null || shellTileType == null)
                {
                    return;
                }

                // Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
                var c = flipTileDataType.GetConstructor(new Type[] { });
                if (c == null)
                {
                    return;
                }

                var updateTileData = c.Invoke(null);

                // Set the properties. 
                SetProperty(updateTileData, "Title", title ?? string.Empty);
                SetProperty(updateTileData, "Count", count);
                SetProperty(updateTileData, "BackTitle", backTitle ?? string.Empty);
                SetProperty(updateTileData, "BackContent", backContent ?? string.Empty);
                SetProperty(updateTileData, "SmallBackgroundImage", smallBackgroundImage ?? ResetUrl);
                SetProperty(updateTileData, "BackgroundImage", backgroundImage ?? ResetUrl);
                SetProperty(updateTileData, "BackBackgroundImage", backBackgroundImage ?? ResetUrl);
                SetProperty(updateTileData, "WideBackgroundImage", wideBackgroundImage ?? ResetUrl);
                SetProperty(updateTileData, "WideBackBackgroundImage", wideBackBackgroundImage ?? ResetUrl);
                SetProperty(updateTileData, "WideBackContent", wideBackContent ?? string.Empty);

                // Invoke the new version of ShellTile.Update.
                shellTileType.GetMethod("Update").Invoke(tileToUpdate, new[] { updateTileData });
            }
            else
            {
                var newAppTile = new StandardTileData
                {
                    BackContent = backContent ?? string.Empty,
                    Count = count,
                    BackTitle = backTitle ?? string.Empty,
                    BackBackgroundImage = backBackgroundImage ?? ResetUrl,
                    BackgroundImage = backgroundImage ?? ResetUrl,
                    Title = title ?? string.Empty
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
            return new Uri("/PlayPage.xaml?mix=" + mix.Id + "&title=" + Uri.EscapeDataString(mix.Name), UriKind.Relative);
        }

        public static void PinToStart(MixContract mix)
        {
            if (!IsPinned(mix))
            {
                var tileUrl = GetPlayPageUrl(mix);

                if (PlatformHelper.IsWindowsPhone78OrLater)
                {
                    // Get the new FlipTileData type.
                    Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");

                    // Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
                    Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
                    if (flipTileDataType == null || shellTileType == null)
                    {
                        return;
                    }

                    // Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
                    var c = flipTileDataType.GetConstructor(new Type[] { });
                    if (c == null)
                    {
                        return;
                    }

                    var updateTileData = c.Invoke(null);
                    // UpdateFlipTile(mix.Name, mix.Name, mix.Description, mix.Description, mix.TrackCount, tileUrl, mix.Cover.ThumbnailUrl, mix.Cover.ThumbnailUrl, null, mix.Cover.OriginalUrl, null, false);


                    // Set the properties. 
                    SetProperty(updateTileData, "Title", mix.Name ?? string.Empty);
                    SetProperty(updateTileData, "Count", mix.TrackCount);
                    SetProperty(updateTileData, "BackTitle", mix.Name ?? string.Empty);
                    SetProperty(updateTileData, "BackContent", mix.Description ?? string.Empty);
                    SetProperty(updateTileData, "SmallBackgroundImage", mix.Cover.ThumbnailUrl ?? ResetUrl);
                    SetProperty(updateTileData, "BackgroundImage", mix.Cover.ThumbnailUrl ?? ResetUrl);
                    SetProperty(updateTileData, "BackBackgroundImage", ResetUrl);
                    SetProperty(updateTileData, "WideBackgroundImage", mix.Cover.OriginalUrl ?? ResetUrl);
                    SetProperty(updateTileData, "WideBackBackgroundImage", ResetUrl);
                    SetProperty(updateTileData, "WideBackContent", mix.Description ?? string.Empty);

                    // Invoke the new version of ShellTile.Update.
                    var createMethod = shellTileType.GetMethods().FirstOrDefault(m => m.Name == "Create" && m.GetParameters().Length == 3);
                    if (createMethod != null)
                    {
                        createMethod.Invoke(null, new[] { tileUrl, updateTileData, true });
                        return;
                    }
                }

                // Fallback to the WinPhone 7.0 way
                ShellTile.Create(
                        tileUrl,
                        new StandardTileData
                            {
                                Title = mix.Name ?? string.Empty,
                                BackContent = mix.Description ?? string.Empty,
                                BackgroundImage = mix.Cover.ThumbnailUrl ?? ResetUrl,
                                BackTitle = mix.Name ?? string.Empty,
                                BackBackgroundImage = ResetUrl, 
                                Count = mix.TrackCount
                            });
                
            }
        }

        public static void UnpinFromStart(MixContract mix)
        {
            var tile = ShellTile.ActiveTiles.FirstOrDefault(t => t.NavigationUri.ToString() == GetPlayPageUrl(mix).ToString());
            if (tile != null)
            {
                tile.Delete();
            }
        }
    }
}
