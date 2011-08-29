namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SetContract
    {
        [DataMember(Name = "at_beginning")]
        public bool IsFirstTrack { get; set; }

        [DataMember(Name = "at_end")]
        public bool IsLastTrack { get; set; }

        [DataMember(Name = "skip_allowed")]
        public bool SkipAllowed { get; set; }

        [DataMember(Name = "track")]
        public TrackContract Track { get; set; }
    }
}