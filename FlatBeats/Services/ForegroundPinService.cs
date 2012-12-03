

namespace FlatBeats.Services
{
    using System;
    using System.Linq;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Shell;

    public sealed class ForegroundPinService : BackgroundPinService
    {
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
    }
}
