namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class MixContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "cover_urls")]
        public CoverUrlContract Cover { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "tag_list_cache")]
        public string Tags { get; set; }

        [DataMember(Name = "restful_url")]
        public string RestUrl { get; set; }

        [DataMember(Name = "liked_by_current_user")]
        public bool Liked { get; set; }

        [DataMember(Name = "user")]
        public UserContract User { get; set; }

        [DataMember(Name = "first_published_at")]
        public string Created { get; set; }
    }
}