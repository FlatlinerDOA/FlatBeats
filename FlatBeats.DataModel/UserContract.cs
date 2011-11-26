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

        [DataMember(Name = "followed_by_current_user")]
        public bool IsFollowed { get; set; }

        [DataMember(Name = "bio_html")]
        public string BioHtml { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

    }
}