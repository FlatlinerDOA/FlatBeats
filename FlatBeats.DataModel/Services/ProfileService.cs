namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.Serialization;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;

    public static class ProfileService
    {
        private const string LikedMixesCacheFile = "{0}\\liked-{1}.json";

        private const string MixFeedCacheFile = "{0}\\mixfeed-{1}.json";

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
                            let user = Json<UserLoginResponseContract>.Deserialize(response)
                            where !string.IsNullOrWhiteSpace(user.UserToken)
                            select user;

            return userLogin.Do(response =>
                {
                    PlayerService.DeletePlayToken();
                    SaveCredentials(userCredentials);
                    SaveUserToken(response);
                    Downloader.UserToken = response.UserToken;
                    Downloader.UserCredentials = userCredentials;
                });
        }

        private static void SaveCredentials(UserCredentialsContract userCredentials)
        {
            Storage.Save(CredentialsFilePath, Json<UserCredentialsContract>.Serialize(userCredentials));
        }

        public static IObservable<UserCredentialsContract> LoadCredentials()
        {
            return ObservableEx.DeferredStart(() => Json<UserCredentialsContract>.Deserialize(Storage.Load(CredentialsFilePath))).Where(c => c != null).Do(c => Downloader.UserCredentials = c);
        }

        private static void SaveUserToken(UserLoginResponseContract login)
        {
            Storage.Save(UserLoginFilePath, Json<UserLoginResponseContract>.Serialize(login));
        }

        public static IObservable<UserLoginResponseContract> LoadUserToken()
        {
            return ObservableEx.DeferredStart(() => Json<UserLoginResponseContract>.Deserialize(Storage.Load(UserLoginFilePath))).Where(c => c != null).Do(
                    user => Downloader.UserToken = user.UserToken);
        }

        public static IObservable<UserProfileResponseContract> GetUserProfile(string userId)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}.json", userId);
            return
                Downloader.GetJson<UserProfileResponseContract>(
                    new Uri(urlFormat, UriKind.Absolute));
        }

        public static IObservable<MixesResponseContract> GetUserMixes(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/mixes.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetJson<MixesResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<MixesResponseContract> GetLikedMixes(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/mixes.json?view=liked&page={1}&per_page={2}", userId, pageNumber, pageSize);
            var cacheFile = string.Format(LikedMixesCacheFile, userId, pageNumber);
            return Downloader.GetJsonCachedAndRefreshed<MixesResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute), cacheFile);
        }

        public static IObservable<MixesResponseContract> GetMixFeed(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/mixes.json?view=mix_feed&page={1}&per_page={2}", userId, pageNumber, pageSize);
            var cacheFile = string.Format(MixFeedCacheFile, userId, pageNumber);
            return Downloader.GetJsonCachedAndRefreshed<MixesResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute), cacheFile);
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
            PlayerService.StopAsync(null, TimeSpan.Zero).Subscribe();
            PlayerService.ClearRecentlyPlayed();
            PlayerService.DeletePlayToken();
            Storage.Delete(UserLoginFilePath);
            Storage.Delete(CredentialsFilePath);
            Downloader.UserToken = null;
            Downloader.UserCredentials = null;
        }

        public static IObservable<ReviewsResponseContract> GetReviewsByUser(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/reviews.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetJson<ReviewsResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<FollowingUserResponseContract> GetFollowedByUsers(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/followed_by_users.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetJson<FollowingUserResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));

        }

        public static IObservable<FollowingUserResponseContract> GetFollowsUsers(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/follows_users.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetJson<FollowingUserResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<FollowUserResponseContract> SetFollowUser(string userId, bool isFollowed)
        {
            var urlFormat = isFollowed ? 
                string.Format("http://8tracks.com/users/{0}/follow.json", userId) : 
                string.Format("http://8tracks.com/users/{0}/unfollow.json", userId);
            var url = new Uri(urlFormat, UriKind.Absolute);
            return Downloader.PostStringAndGetJson<FollowUserResponseContract>(url, string.Empty);
        }

        public static IObservable<ReviewResponseContract> AddMixReview(string mixId, string review)
        {
            var url = new Uri("http://8tracks.com/reviews", UriKind.Absolute);
            string body = string.Format("review%5Bbody%5D={0}&review%5Bmix_id%5D={1}&format=json", HttpUtility.UrlEncode(review), mixId);
            return Downloader.PostStringAndGetJson<ReviewResponseContract>(url, body).Select(
                response =>
                {
                    if (response.Review.User == null)
                    {
                        response.Review.User =
                            ProfileService.GetUserProfile(response.Review.UserId).Select(up => up.User).FirstOrDefault();
                    }

                    return response;
                });
        }

        public static IObservable<Unit> ResetAsync()
        {
            return ObservableEx.DeferredStart(DeleteCredentials);
        }
    }

    [DataContract]
    public class FollowingUserResponseContract : ResponseContract
    {
        [DataMember(Name = "users")]
        public List<UserContract> Users { get; set; }
    }
}
