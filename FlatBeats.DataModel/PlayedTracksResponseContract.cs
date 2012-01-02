namespace FlatBeats.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class PlayedTracksResponseContract
    {
        /// <summary>
        /// Initializes a new instance of the PlayedTracksResponseContract class.
        /// </summary>
        public PlayedTracksResponseContract()
        {
            this.Tracks = new List<TrackContract>();
        }
        //// http://8tracks.com/sets/460486803/tracks_played.xml?mix_id=2000
        
        [DataMember(Name = "tracks")]
        public List<TrackContract> Tracks { get; set; }
    }
}
