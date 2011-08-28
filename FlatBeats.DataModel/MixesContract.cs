namespace FlatBeats.DataModel
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class MixesContract
    {
        [DataMember(Name = "mixes")]
        public List<MixContract> Mixes { get; set; }
    }
}