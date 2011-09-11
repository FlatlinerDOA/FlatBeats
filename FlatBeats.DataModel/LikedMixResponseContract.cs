namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class LikedMixResponseContract
    {
        [DataMember(Name = "mix")]
        public LikedMixContract Mix { get; set; }
    }
}