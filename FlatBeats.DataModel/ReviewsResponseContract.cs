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

namespace FlatBeats.DataModel
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ReviewsResponseContract
    {
        [DataMember(Name = "reviews")]
        public List<ReviewContract> Reviews { get; set; }
    }

    [DataContract]
    public class ReviewResponseContract
    {
        [DataMember(Name = "review")]
        public ReviewContract Review { get; set; }
    }
}
