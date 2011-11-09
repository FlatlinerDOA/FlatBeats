namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class UserProfileResponseContract
    {
        [DataMember(Name = "user")]
        public UserContract User { get; set; }
    }
}