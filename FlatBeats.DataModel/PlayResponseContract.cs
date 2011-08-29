

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
}
