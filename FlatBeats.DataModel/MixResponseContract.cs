using System;

namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class MixResponseContract
    {
        [DataMember(Name = "mix")]
        public MixContract Mix { get; set; }
    }
}
