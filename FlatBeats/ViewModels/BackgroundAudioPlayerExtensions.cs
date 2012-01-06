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

namespace FlatBeats.ViewModels
{
    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;

    public static class BackgroundAudioPlayerExtensions
    {
        public static IObservable<PlayState> PlayStateChanges(this BackgroundAudioPlayer player)
        {
            var events = Observable.FromEvent<EventArgs>(player, "PlayStateChanged").Select(_ => player.PlayerState);
            return events;
        }

        /// <summary>
        /// Ignores the play state, only checks if there is a current track
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsPlayingATrack(this BackgroundAudioPlayer player)
        {
            if (player == null)
            {
                return false;
            }

            try
            {
                return player.Track != null;
            } 
            catch (Exception)
            {
                return false;
            }
        }
    }
}
