namespace FlatBeats.DataModel
{
    using System.Runtime.Serialization;

    [DataContract]
    public class UserLoginResponseContract
    {
        [DataMember(Name = "current_user")]
        public UserContract CurentUser { get; set; }

        [DataMember(Name = "user_token")]
        public string UserToken { get; set; }

        /*
         {"current_user":
         {"popup_prefs":"ask","next_mix_prefs":"ask","bio_html":null,"location":"","subscribed":null,"follows_count":0,"id":532219,"login":"FlatlinerDOA","slug":"flatlinerdoa","path":"/flatlinerdoa","restful_url":"http://8tracks.com/users/532219","avatar_urls":
              {"sq56":"http://cf3.8tracks.us/avatars/000/532/219/25569.sq56.jpg","sq72":"http://cf3.8tracks.us/avatars/000/532/219/25569.sq72.jpg","sq100":"http://cf3.8tracks.us/avatars/000/532/219/25569.sq100.jpg","max200":"http://cf3.8tracks.us/avatars/000/532/219/25569.max200.jpg"
              }
         },
         "auth_token":"532219;f1d1ec642b3d37808a19c3c32038d104458578e6",
         "user_token":"532219;f1d1ec642b3d37808a19c3c32038d104458578e6",
         "redirect_to":"/",
         "status":"200 OK",
         "errors":null,
         "notices":"You are now logged in as FlatlinerDOA",
         "logged_in":true}
<response>
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