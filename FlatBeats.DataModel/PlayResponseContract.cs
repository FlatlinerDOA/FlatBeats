

namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class PlayResponseContract
    {
        [DataMember(Name = "set")]
        public SetContract Set { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }
    }

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

    [DataContract]
    public class TrackContract
    {
        [DataMember(Name = "faved_by_current_user")]
        public bool IsFavourite { get; set; }

        [DataMember(Name = "year")]
        public string Year { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "performer")]
        public string Artist { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "url")]
        public string TrackUrl { get; set; }
    }
}
