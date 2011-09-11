namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    public class UserLoginResponseContract
    {
        [DataMember(Name = "user_token")]
        public string UserToken { get; set; }

        [DataMember(Name = "current_user")]
        public UserContract CurentUser { get; set; }
        /*<response>
  <user-token>1;123456789</user-token>
  <current-user>
    <slug>remitest</slug>
    <location></location>
    <popup-prefs>ask</popup-prefs>
    <next-mix-prefs>ask</next-mix-prefs>
    <avatar-urls>
      <sq72>/images/avatars/sq72.jpg</sq72>
      <sq100>/images/avatars/sq100.jpg</sq100>
      <max200>/images/avatars/max200.jpg</max200>
      <sq56>/images/avatars/sq56.jpg</sq56>
    </avatar-urls>
    <bio-html nil="true"></bio-html>
    <subscribed nil="true"></subscribed>
    <login>remitest</login>
    <id>988</id>
    <followed-by-current-user>false</followed-by-current-user>
  </current-user>
  <notices nil="true"></notices>
  <errors nil="true"></errors>
  <status>200 OK</status>
  <logged-in>true</logged-in>
</response>
*/
    }
}