namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class FavouritedTrackResponseContract
    {
        [DataMember(Name = "track")]
        public FavouritedTrackContract Track { get; set; }
    }
}