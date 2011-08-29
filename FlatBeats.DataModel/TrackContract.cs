namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TrackContract
    {
        [DataMember(Name = "faved_by_current_user")]
        public bool IsFavourite { get; set; }

        [DataMember(Name = "year")]
        public string Year { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "performer")]
        public string Artist { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "url")]
        public string TrackUrl { get; set; }
    }
}