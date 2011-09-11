namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class LikedMixContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name ="liked_by_current_user")]
        public bool IsLiked { get; set; }
    }
}