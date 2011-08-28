
namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class PlayTokenResponseContract
    {
        [DataMember(Name = "play_token")]
        public string PlayToken { get; set; }
    }
}
