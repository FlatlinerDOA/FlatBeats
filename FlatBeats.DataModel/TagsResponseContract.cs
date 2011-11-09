namespace FlatBeats.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class TagsResponseContract
    {
        [DataMember(Name = "tags")]
        public List<TagContract> Tags { get; set; }
    }
}
