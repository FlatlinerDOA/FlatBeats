namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class PlayingMixContract
    {
        [DataMember(Name = "play_token")]
        public string PlayToken { get; set; }

        [DataMember(Name = "mix_id")]
        public string MixId { get; set; }

        [DataMember(Name = "mix_name")]
        public string MixName { get; set; }

        [DataMember(Name = "set")]
        public SetContract Set { get; set; }

        [DataMember(Name = "cover_url")]
        public CoverUrlContract Cover { get; set; }

        [DataMember(Name = "mix_set_id")]
        public string MixSetId { get; set; }
    }
}
