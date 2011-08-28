using System;
using Microsoft.Phone.BackgroundAudio;

namespace FlatBeatsPlaybackAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            switch (playState)
            {
                case PlayState.TrackReady:
                    // The track to play is set in the PlayTrack method.
                    player.Play();
                    break;

                case PlayState.TrackEnded:
                    this.PlayNextTrack(player);
                    break;
            }

            this.NotifyComplete();
        }


        /// <summary>
        /// Called when the user requests an action using system-provided UI and the application has requesed
        /// notifications of the action
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            //TODO: Add code to handle user actions through the application and system-provided UI
            switch (action)
            {
                case UserAction.Stop:
                    break;
                case UserAction.Pause:
                    break;
                case UserAction.Play:
                    break;
                case UserAction.SkipNext:
                    break;
                case UserAction.SkipPrevious:
                    break;
                case UserAction.FastForward:
                    break;
                case UserAction.Rewind:
                    break;
                case UserAction.Seek:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }

            this.NotifyComplete();
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            // TODO: Add code to handle error conditions

            this.NotifyComplete();
        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        protected override void OnCancel()
        {
        }


        /// <summary>
        /// Increments the currentTrackNumber and plays the correpsonding track.
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        private void PlayNextTrack(BackgroundAudioPlayer player)
        {
            ////if (++currentTrackNumber >= _playList.Count)
            ////{
            ////    currentTrackNumber = 0;
            ////}

            this.PlayTrack(player);
        }

        private void PlayTrack(BackgroundAudioPlayer player)
        {
            if (PlayState.Paused == player.PlayerState)
            {
                // If we're paused, we already have 
                // the track set, so just resume playing.
                player.Play();
            }
            else
            {
                // Set which track to play. When the TrackReady state is received 
                // in the OnPlayStateChanged handler, call player.Play().
                player.Track = new AudioTrack();
            }
        }
    }
}
