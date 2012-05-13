
namespace FlatBeats.Tracks.DataModel
{
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class VideoFeedContract
    {
        [DataMember(Name = "feed")]
        public FeedContract Feed { get; set; }
    }

    [DataContract]
    public class FeedContract
    {
        [DataMember(Name = "entry")]
        public Collection<EntryContract> Entries { get; set; }
    }

    [DataContract]
    public class EntryContract
    {
        [DataMember(Name = "title")]
        public TextContract Title { get; set; }
        
        [DataMember(Name = "content")]
        public TextContract Content { get; set; }

        [DataMember(Name = "media$content")]
        public Collection<MediaContentContract> MediaContent { get; set; }

        [DataMember(Name = "media$player")]
        public MediaPlayerLinkContract MediaPlayer { get; set; }

        [DataMember(Name = "media$thumbnail")]
        public Collection<MediaThumbnailContract> MediaThumbnails { get; set; }

    }
    
    [DataContract]
    public class MediaThumbnailContract
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }


    }

    [DataContract]
    public class MediaPlayerLinkContract
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }

    }

    [DataContract]
    public class TextContract
    {
        [DataMember(Name = "$t")]
        public string Text { get; set; }

        [DataMember(Name = "type")]
        public string TextType { get; set; }
    }

    [DataContract]
    public class MediaContentContract
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "type")]
        public string ContentType { get; set; }

        [DataMember(Name = "duration")]
        public int DurationInSeconds { get; set; }

        [DataMember(Name = "yt$format")]
        public string Format { get; set; }
    }
}
