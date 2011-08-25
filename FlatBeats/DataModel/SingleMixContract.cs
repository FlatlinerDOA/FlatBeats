using System;

namespace EightTracks.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SingleMixContract
    {
        [DataMember(Name = "mix")]
        public MixContract Mix { get; set; }
    }
}
