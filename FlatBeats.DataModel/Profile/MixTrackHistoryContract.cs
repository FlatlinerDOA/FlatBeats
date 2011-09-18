using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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
        [DataMember(Name = "id")]
        public int MixId { get; set; }

        [DataMember(Name = "played")]
        public List<TrackContract> PlayedTracks { get; set; }
    }
}
