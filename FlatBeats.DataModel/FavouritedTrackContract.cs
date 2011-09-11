namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class FavouritedTrackContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "faved_by_current_user")]
        public bool IsFavourited { get; set; }
    }
}