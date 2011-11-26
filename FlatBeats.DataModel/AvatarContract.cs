namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AvatarContract
    {
        [DataMember(Name = "sq100")]
        public string ImageUrl { get; set; }

        [DataMember(Name = "max200")]
        public string LargeImageUrl { get; set; }
    }
}