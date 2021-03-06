﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.DataModel
{
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class FavouritedTrackListResponseContract : ResponseContract
    {
        [DataMember(Name = "tracks")]
        public Collection<TrackContract> Tracks { get; set; }
    }
}
