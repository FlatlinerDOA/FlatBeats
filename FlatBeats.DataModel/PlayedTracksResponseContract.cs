namespace FlatBeats.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class PlayedTracksResponseContract
    {
        //// http://8tracks.com/sets/460486803/tracks_played.xml?mix_id=2000
        
        [DataMember(Name = "tracks")]
        public List<TrackContract> Tracks { get; set; }
    }
}
