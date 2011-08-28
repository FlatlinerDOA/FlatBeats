namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class MixContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "cover_urls")]
        public CoverUrlContract CoverUrls { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }


        [DataMember(Name = "description")]
        public string Description { get; set; }


        [DataMember(Name = "tag_list_cache")]
        public string Tags { get; set; }
    }
}