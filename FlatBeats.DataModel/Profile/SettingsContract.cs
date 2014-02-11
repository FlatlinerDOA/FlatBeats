// --------------------------------------------------------------------------------------------------
//  <copyright file="SettingsContract.cs" company="DNS Technology Pty Ltd.">
//    Copyright (c) 2014 DNS Technology Pty Ltd. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------
namespace FlatBeats.DataModel.Profile
{
    using System.Runtime.Serialization;

    /// <summary>
    /// </summary>
    [DataContract]
    public class SettingsContract
    {
        #region Public Properties

        /// <summary>
        /// </summary>
        [DataMember(Name = "censorship_enabled")]
        public bool CensorshipEnabled { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "music_store")]
        public string MusicStore { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "play_next_mix")]
        public bool PlayNextMix { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "play_over_wifi_only")]
        public bool PlayOverWifiOnly { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "preferred_list")]
        public string PreferredList { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "prompted_to_rate")]
        public bool PromptedToRate { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "start_count")]
        public int StartCount { get; set; }

        #endregion
    }
}
