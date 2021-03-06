﻿namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class MixContract
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "cover_urls")]
        public CoverUrlContract Cover { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "tag_list_cache")]
        public string Tags { get; set; }

        [DataMember(Name = "restful_url")]
        public string RestUrl { get; set; }

        [DataMember(Name = "liked_by_current_user")]
        public bool Liked { get; set; }

        [DataMember(Name = "user")]
        public UserContract User { get; set; }

        [DataMember(Name = "first_published_at")]
        public string Created { get; set; }

        [DataMember(Name = "plays_count")]
        public int PlaysCount { get; set; }

        [DataMember(Name = "likes_count")]
        public int LikesCount { get; set; }

        [DataMember(Name = "nsfw")]
        public bool IsExplicit { get; set; }

        [DataMember(Name = "duration")]
        public int DurationSeconds { get; set; }

        [DataMember(Name = "tracks_count")]
        public int TrackCount { get; set; }
    }
}