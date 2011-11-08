namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ResponseContract
    {
        [DataMember(Name = "status")]
        public string ResponseStatus { get; set; }

        [DataMember(Name = "errors")]
        public string Errors { get; set; }

        [DataMember(Name = "notices")]
        public string Notices { get; set; }
        
        [DataMember(Name = "logged_in")]
        public bool IsLoggedIn { get; set; }
    }
}
