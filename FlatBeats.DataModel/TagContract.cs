namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TagContract
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "cool_taggings_count")]
        public string Count { get; set; }
    }
}