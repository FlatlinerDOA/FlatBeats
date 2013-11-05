namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class CoverUrlContract
    {
        [DataMember(Name = "sq133")]
        public Uri ThumbnailUrl { get; set; }

        [DataMember(Name = "original_imgix_url")]
        public Uri OriginalUrl { get; set; }

        [DataMember(Name = "max200")]
        public Uri Max200Url { get; set; }

            /*-<cover-urls> 
         * <sq56>http://cf1.8tracks.us/mix_covers/000/367/837/1402.sq56.jpg</sq56> 
         * <sq100>http://cf1.8tracks.us/mix_covers/000/367/837/1402.sq100.jpg</sq100> 
         * <sq133>http://cf1.8tracks.us/mix_covers/000/367/837/1402.sq133.jpg</sq133> 
         * <max133w>http://cf1.8tracks.us/mix_covers/000/367/837/1402.max133w.jpg</max133w> 
         * <max200>http://cf1.8tracks.us/mix_covers/000/367/837/1402.max200.jpg</max200> 
         * <original>http://cf1.8tracks.us/mix_covers/000/367/837/1402.original.JPG</original> 
         * </cover-urls>*/
        
        /* Updated 2013-11-05
        {
        "sq56":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?fm=jpg&q=65&w=56&h=56&fit=max",
        "sq100":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?fm=jpg&q=65&w=100&h=100&fit=max",
        "sq133":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?fm=jpg&q=65&w=133&h=133&fit=max",
        "max133w":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?fm=jpg&q=65&w=133&fit=max",
        "max200":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?fm=jpg&q=65&w=200&h=200&fit=max",
        "sq250":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?fm=jpg&q=65&w=250&h=250&fit=max",
        "sq500":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?fm=jpg&q=65&w=500&h=500&fit=max",
        "max1024":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?fm=jpg&q=65&w=1024&h=1024&fit=max",
        "original_imgix_url":"http://8tracks.imgix.net/i/000/717/087/safe_image-7378.jpg?q=65&sharp=15&vib=10&fm=jpg&fit=crop",
        "animated":false
        }*/
    }
}