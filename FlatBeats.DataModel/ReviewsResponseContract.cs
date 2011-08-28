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
    public class ReviewContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        
        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "user")]
        public UserContract User { get; set; }
    }

    [DataContract]
    public class UserContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "login")]
        public string Name { get; set; }

        [DataMember(Name = "avatar_urls")]
        public AvatarContract Avatar { get; set; }
    }

    [DataContract]
    public class AvatarContract
    {
        [DataMember(Name = "sq100")]
        public string ImageUrl { get; set; }
    }
}
