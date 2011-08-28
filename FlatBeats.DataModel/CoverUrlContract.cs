namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class CoverUrlContract
    {
        [DataMember(Name = "sq133")]
        public Uri ThumbnailUrl { get; set; }

        [DataMember(Name = "original")]
        public Uri OriginalUrl { get; set; 

            /*-<cover-urls> 
         * <sq56>http://cf1.8tracks.us/mix_covers/000/367/837/1402.sq56.jpg</sq56> 
         * <sq100>http://cf1.8tracks.us/mix_covers/000/367/837/1402.sq100.jpg</sq100> 
         * <sq133>http://cf1.8tracks.us/mix_covers/000/367/837/1402.sq133.jpg</sq133> 
         * <max133w>http://cf1.8tracks.us/mix_covers/000/367/837/1402.max133w.jpg</max133w> 
         * <max200>http://cf1.8tracks.us/mix_covers/000/367/837/1402.max200.jpg</max200> 
         * <original>http://cf1.8tracks.us/mix_covers/000/367/837/1402.original.JPG</original> 
         * </cover-urls>*/
        }
    }
}