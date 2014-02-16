// --------------------------------------------------------------------------------------------------
//  <copyright file="MixTrackHistoryContract.cs" company="Andrew Chisholm">
//    Copyright (c) 2014 Andrew Chisholm. All rights reserved.
//  </copyright>
// --------------------------------------------------------------------------------------------------
namespace FlatBeats.DataModel.Profile
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// The list of tracks played locally on the device, does not rely on the service's track history.
    /// </summary>
    [DataContract]
    public class MixTrackHistoryContract
    {
        #region Public Properties

        /// <summary>
        /// </summary>
        [DataMember(Name = "id")]
        public int MixId { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Name = "played")]
        public List<TrackContract> PlayedTracks { get; set; }

        #endregion
    }
}
