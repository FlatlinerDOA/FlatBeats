﻿namespace FlatBeats.DataModel.Services
{
    using System;
    using Microsoft.Phone.Reactive;

    public static class ProfileService
    {
        private const string CredentialsFilePath = "credentials.json";

        private const string UserLoginFilePath = "userlogin.json";

        public static IObservable<UserLoginResponseContract> Authenticate(UserCredentialsContract userCredentials)
        {
            if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.UserName) || string.IsNullOrWhiteSpace(userCredentials.Password))
            {
                return Observable.Empty<UserLoginResponseContract>();
            }

            Downloader.UserCredentials = null;
            Downloader.UserToken = null;
            var url = new Uri("https://8tracks.com/sessions.json", UriKind.Absolute);
            var postData = string.Format(
                "login={0}&password={1}", 
                Uri.EscapeDataString(userCredentials.UserName), 
                Uri.EscapeDataString(userCredentials.Password));
            var userLogin = from response in Downloader.PostAndGetString(url, postData)
                            let user = Json.Deserialize<UserLoginResponseContract>(response)
                            where !string.IsNullOrWhiteSpace(user.UserToken)
                            select user;

            return userLogin.Do(response =>
                {
                    SaveCredentials(userCredentials);
                    SaveUserToken(response);
                    Downloader.UserToken = response.UserToken;
                    Downloader.UserCredentials = userCredentials;
                });
        }

        private static void SaveCredentials(UserCredentialsContract userCredentials)
        {
            Storage.Save(CredentialsFilePath, Json.Serialize(userCredentials));
        }

        public static IObservable<UserCredentialsContract> LoadCredentials()
        {
            return Observable.Start(() => Json.Deserialize<UserCredentialsContract>(Storage.Load(CredentialsFilePath))).Where(c => c != null).Do(c => Downloader.UserCredentials = c);
        }

        private static void SaveUserToken(UserLoginResponseContract login)
        {
            Storage.Save(UserLoginFilePath, Json.Serialize(login));
        }

        public static IObservable<UserLoginResponseContract> LoadUserToken()
        {
            return
                Observable.Start(() => Json.Deserialize<UserLoginResponseContract>(Storage.Load(UserLoginFilePath))).Where(c => c != null).Do(
                    user => Downloader.UserToken = user.UserToken);
        }


        public static IObservable<MixesResponseContract> GetUserMixes(string userId)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/mixes.json", userId);
            return
                Downloader.GetJson<MixesResponseContract>(
                    new Uri(urlFormat, UriKind.RelativeOrAbsolute));

        }

        public static IObservable<MixesResponseContract> GetLikedMixes(string userId)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/mixes.json?view=liked", userId);
            return
                Downloader.GetJson<MixesResponseContract>(
                    new Uri(urlFormat, UriKind.RelativeOrAbsolute));

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

        private static void DeleteCredentials()
        {
            PlayerService.Stop();
            PlayerService.ClearRecentlyPlayed();
            PlayerService.DeletePlayToken();
            Storage.Delete(UserLoginFilePath);
            Storage.Delete(CredentialsFilePath);
            Downloader.UserToken = null;
            Downloader.UserCredentials = null;
        }

        public static IObservable<Unit> Reset()
        {
            return from deleteCredentials in Observable.Start(DeleteCredentials)
                   select new Unit();
        }
    }
}