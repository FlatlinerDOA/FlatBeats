// --------------------------------------------------------------------------------------------------
//  <copyright file="ForegroundPinService.cs" company="Andrew Chisholm">
//    Copyright (c) 2014 Andrew Chisholm. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------
namespace FlatBeats.Services
{
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Shell;

    /// <summary>
    /// </summary>
    public sealed class ForegroundPinService : BackgroundPinService
    {
        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <param name="mix">
        /// </param>
        public static void PinToStart(MixContract mix)
        {
            if (BackgroundPinService.IsPinned(mix))
            {
                return;
            }

            var tileUrl = GetPlayPageUrl(mix);
            if (tileUrl == null)
            {
                return;
            }

            var updateTileData = new FlipTileData 
            {
                Title = mix.Name ?? string.Empty, 
                Count = mix.TrackCount, 
                BackTitle = mix.Name ?? string.Empty, 
                BackContent = mix.Description ?? string.Empty, 
                SmallBackgroundImage = mix.Cover.ThumbnailUrl ?? ResetUrl, 
                BackgroundImage = mix.Cover.ThumbnailUrl ?? ResetUrl, 
                BackBackgroundImage = ResetUrl, 
                WideBackgroundImage = mix.Cover.OriginalUrl ?? ResetUrl, 
                WideBackBackgroundImage = ResetUrl, 
                WideBackContent = mix.Description ?? string.Empty
            };

            ShellTile.Create(tileUrl, updateTileData, true);
        }

        #endregion
    }
}
