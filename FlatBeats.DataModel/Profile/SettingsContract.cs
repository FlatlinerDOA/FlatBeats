namespace FlatBeats.DataModel.Profile
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class SettingsContract
    {
        [DataMember(Name = "play_over_wifi_only")]
        public bool PlayOverWifiOnly { get; set; }

        [DataMember(Name = "censorship_enabled")]
        public bool CensorshipEnabled { get; set; }

        [DataMember(Name = "play_next_mix")]
        public bool PlayNextMix { get; set; }

        [DataMember(Name = "preferred_list")]
        public string PreferredList { get; set; }
    }
}
