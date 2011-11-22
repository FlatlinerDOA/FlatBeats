namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReviewContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        
        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "user")]
        public UserContract User { get; set; }

        [DataMember(Name = "user_id")]
        public string UserId{ get; set; }

        [DataMember(Name = "created_at")]
        public string Created { get; set; }
    }
}