﻿namespace FlatBeats.DataModel.Profile
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class PlayHistoryContract
    {
        [DataMember(Name = "played")]
        public List<MixContract> PlayedMixes { get; set; }
    }
}
