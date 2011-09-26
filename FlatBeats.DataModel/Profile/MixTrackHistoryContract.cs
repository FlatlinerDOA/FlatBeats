//--------------------------------------------------------------------------------------------------
// <copyright file="MixTrackHistoryContract.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.DataModel.Profile
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// The list of tracks played locally on the device, does not rely on the service's track history.
    /// </summary>
    [DataContract]
    public class MixTrackHistoryContract
    {
        [DataMember(Name = "id")]
        public int MixId { get; set; }

        [DataMember(Name = "played")]
        public List<TrackContract> PlayedTracks { get; set; }
    }
}
