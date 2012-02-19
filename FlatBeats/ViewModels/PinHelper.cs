﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.ViewModels
{
    using System.Linq;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Shell;

    public static class PinHelper
    {
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
    }
}