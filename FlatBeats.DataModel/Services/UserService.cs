namespace FlatBeats.DataModel.Services
{
    using System;
    using Microsoft.Phone.Reactive;

    public static class UserService
    {
        private const string CredentialsFilePath = "credentials.json";
        
        public static IObservable<UserLoginResponseContract> Authenticate(UserCredentialsContract userCredentials)
        {
            if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.UserName) || string.IsNullOrWhiteSpace(userCredentials.Password))
            {
                return Observable.Empty<UserLoginResponseContract>();
            }

            var url = new Uri("https://8tracks.com/sessions.json", UriKind.Absolute);
            var postData = string.Format(
                "login={0}&password={1}", Uri.EscapeDataString(userCredentials.UserName), Uri.EscapeDataString(userCredentials.Password));
            var userLogin = from response in Downloader.PostAndGetString(url, postData)
                   let user = Json.Deserialize<UserLoginResponseContract>(response)
                   where !string.IsNullOrWhiteSpace(user.UserToken)
                   select user;

            return userLogin.Do(_ => SaveCredentials(userCredentials));
        }

        private static void SaveCredentials(UserCredentialsContract userCredentials)
        {
            Storage.Save(CredentialsFilePath, Json.Serialize(userCredentials));
        }

        public static IObservable<UserCredentialsContract> LoadCredentials()
        {
            return Observable.Start(
                () => Json.Deserialize<UserCredentialsContract>(Storage.Load(CredentialsFilePath))).Where(c => c != null);
        }

        public static IObservable<MixesResponseContract> GetLikedMixes()
        {
            return
                Downloader.GetJson<MixesResponseContract>(
                    new Uri("http://8tracks.com/users/dp/mixes.xml?view=liked", UriKind.RelativeOrAbsolute));
        }

        public static IObservable<Unit> SetMixLiked(string mixId, bool isLiked)
        {
            var urlFormat = isLiked ? string.Format("http://8tracks.com/mixes/{0}/like.json", mixId) : string.Format("http://8tracks.com/mixes/{0}/unlike.json", mixId);
            return Downloader.PostStringAndGetJson<LikedMixResponseContract>(new Uri(urlFormat, UriKind.Absolute), string.Empty).Select(r => new Unit());
        }

        public static IObservable<Unit> SetTrackFavourite(string trackId, bool isFavourite)
        {
            var urlFormat = isFavourite ? string.Format("http://8tracks.com/tracks/{0}/fav.json", trackId) : string.Format("http://8tracks.com/tracks/{0}/unfav.json", trackId);
            return Downloader.PostStringAndGetJson<FavouritedTrackResponseContract>(new Uri(urlFormat, UriKind.Absolute), string.Empty).Select(r => new Unit());
        }
    }
}
