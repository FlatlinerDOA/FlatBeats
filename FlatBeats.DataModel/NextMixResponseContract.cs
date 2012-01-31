namespace FlatBeats.DataModel
{
    using System;
    using System.Runtime.Serialization;

    /*<response> 
  <status>200 OK</status> 
  <notices nil="true"></notices> 
  <errors nil="true"></errors> 
  <next-mix> 
    <user> 
      <avatar-urls> 
        <sq100>http://cf0.8tracks.us/avatars/000/014/896/99417.sq100.jpg</sq100> 
        <max200>http://cf0.8tracks.us/avatars/000/014/896/99417.max200.jpg</max200> 
        <sq56>http://cf0.8tracks.us/avatars/000/014/896/99417.sq56.jpg</sq56> 
        <sq72>http://cf0.8tracks.us/avatars/000/014/896/99417.sq72.jpg</sq72> 
      </avatar-urls> 
      <slug>cardioidcochlea</slug> 
      <followed-by-current-user>false</followed-by-current-user> 
      <login>cardioidcochlea</login> 
      <id>14896</id> 
    </user> 
    <first-published-at>2010-06-29T04:06:23Z</first-published-at> 
    <path>/cardioidcochlea/little-expressionless-animals-x</path> 
    <plays-count>61</plays-count> 
    <slug>little-expressionless-animals-x</slug> 
    <liked-by-current-user>false</liked-by-current-user> 
    <description></description> 
    <name>Little Expressionless Animals X</name> 
    <restful-url>http://8tracks.com/mixes/129928</restful-url> 
    <id>129928</id> 
    <tag-list-cache></tag-list-cache> 
    <cover-urls> 
      <sq100>http://cf0.8tracks.us/mix_covers/000/129/928/70646.sq100.jpg</sq100> 
      <max200>http://cf0.8tracks.us/mix_covers/000/129/928/70646.max200.jpg</max200> 
      <sq133>http://cf0.8tracks.us/mix_covers/000/129/928/70646.sq133.jpg</sq133> 
      <max133w>http://cf0.8tracks.us/mix_covers/000/129/928/70646.max133w.jpg</max133w> 
      <sq56>http://cf0.8tracks.us/mix_covers/000/129/928/70646.sq56.jpg</sq56> 
      <original>http://cf0.8tracks.us/mix_covers/000/129/928/70646.original.png</original> 
    </cover-urls> 
    <published>true</published> 
  </next-mix> 
  <logged-in>false</logged-in> 
</response>*/

    [DataContract]
    public class NextMixResponseContract : ResponseContract
    {
        [DataMember(Name = "next_mix")]
        public MixContract NextMix { get; set; }
    }
}
