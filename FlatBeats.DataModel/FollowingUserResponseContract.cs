namespace FlatBeats.DataModel
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class FollowingUserResponseContract : ResponseContract
    {
        [DataMember(Name = "users")]
        public List<UserContract> Users { get; set; }
    }
}