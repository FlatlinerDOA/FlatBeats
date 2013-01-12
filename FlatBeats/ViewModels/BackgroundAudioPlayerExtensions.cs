using System;

namespace FlatBeats.ViewModels
{
    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;

    public static class BackgroundAudioPlayerExtensions
    {
        public static IObservable<PlayState> PlayStateChanges(this BackgroundAudioPlayer player)
        {
            var events = Observable.FromEvent<EventArgs>(player, "PlayStateChanged").Select(_ =>
                {
                    try
                    {
                        return player.PlayerState;
                    }
                    catch (InvalidOperationException)
                    {
                        return PlayState.Error;
                    }
                });
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
