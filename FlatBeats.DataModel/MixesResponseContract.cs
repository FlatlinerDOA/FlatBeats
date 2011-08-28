namespace FlatBeats.DataModel
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class MixesResponseContract
    {
        [DataMember(Name = "mixes")]
        public List<MixContract> Mixes { get; set; }
    }
}