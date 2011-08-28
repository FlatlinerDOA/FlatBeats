namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class UserContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "login")]
        public string Name { get; set; }

        [DataMember(Name = "avatar_urls")]
        public AvatarContract Avatar { get; set; }
    }
}