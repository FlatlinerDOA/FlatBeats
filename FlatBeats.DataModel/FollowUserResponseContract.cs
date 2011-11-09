namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class FollowUserResponseContract
    {
        [DataMember(Name = "user")]
        public UserContract User { get; set; }
    }
}