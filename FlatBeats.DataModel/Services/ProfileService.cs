namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Net;
    using System.Windows.Interop;

    using FlatBeats.DataModel.Profile;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;
    using System.Collections.Generic;

    public class CurrentUserProfileService : IProfileService
    {
        private bool isInitialized;

        public string UserId { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public CurrentUserProfileService()
        {
        }

        public IObservable<Unit> InitializeAsync()
        {
            if (this.isInitialized)
            {
                return Observable.Return(new Unit());
            }

            this.isInitialized = true;
            return ProfileService.LoadUserTokenAsync().Do(u => this.UserId = u.CurrentUser.Id).Select(_ => new Unit());
        }

        public IObservable<IList<MixContract>> GetLikedMixesAsync(int pageNumber, int pageSize)
        {
            return ProfileService.GetLikedMixesAsync(this.UserId, pageNumber, pageSize).Select(m => (IList<MixContract>)m.Mixes);
        }

        public IObservable<Unit> AuthenticateAsync(UserCredentialsContract userCredentials)
        {
            return ProfileService.AuthenticateAsync(userCredentials).Select(_ => new Unit());
        }
    }

    public static class ProfileService
    {
        private const string LikedMixesCacheFile = "/cache/{0}/liked-{1}.json";

        private const string MixFeedCacheFile = "/cache/{0}/mixfeed-{1}.json";

        private const string CredentialsFilePath = "credentials.json";

        private const string SettingsFilePath = "settings.json";

        private const string UserLoginFilePath = "userlogin.json";
        
        public static IObservable<SettingsContract> GetSettingsAsync()
        {
            return from json in Storage.LoadStringAsync(SettingsFilePath)
                   select Json<SettingsContract>.Deserialize(json) ?? new SettingsContract() { CensorshipEnabled = true, PlayNextMix = true, PreferredList = PreferredLists.Liked };
        }

        public static IObservable<Unit> SaveSettingsAsync(SettingsContract settings)
        {
            return Storage.SaveStringAsync(SettingsFilePath, Json<SettingsContract>.Serialize(settings));
        }

        public static IObservable<UserLoginResponseContract> AuthenticateAsync(UserCredentialsContract userCredentials)
        {
            if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.UserName) || string.IsNullOrWhiteSpace(userCredentials.Password))
            {
                return Observable.Empty<UserLoginResponseContract>();
            }
            
            Downloader.UserCredentials = null;
            Downloader.UserToken = null;
            var postData = string.Format(
                "login={0}&password={1}",
                ApiUrl.Escape(userCredentials.UserName),
                ApiUrl.Escape(userCredentials.Password));
            var userLogin = from response in Downloader.PostAndGetString(ApiUrl.Authenticate(), postData)
                            let user = Json<UserLoginResponseContract>.Deserialize(response)
                            where !string.IsNullOrWhiteSpace(user.UserToken)
                            select user;

            return (from loginResponse in userLogin
                   from _ in SaveCredentialsAsync(userCredentials)
                   from __ in SaveUserTokenAsync(loginResponse)
                   select loginResponse)
                       .Do(response =>
                        {
                            PlayerService.DeletePlayToken();
                            Downloader.UserToken = response.UserToken;
                            Downloader.UserCredentials = userCredentials;
                        });
        }

        private static IObservable<Unit> SaveCredentialsAsync(UserCredentialsContract userCredentials)
        {
            return Storage.SaveJsonAsync(CredentialsFilePath, userCredentials);
        }

        public static IObservable<UserCredentialsContract> LoadCredentialsAsync()
        {
            return Storage.LoadJsonAsync<UserCredentialsContract>(CredentialsFilePath).Where(c => c != null).Do(c => Downloader.UserCredentials = c);
        }

        private static IObservable<Unit> SaveUserTokenAsync(UserLoginResponseContract login)
        {
            return Storage.SaveJsonAsync(UserLoginFilePath, login);
        }

        public static IObservable<UserLoginResponseContract> LoadUserTokenAsync()
        {
            return Storage.LoadJsonAsync<UserLoginResponseContract>(UserLoginFilePath).NotNull()
                .Where(c => c.CurrentUser != null)
                .Do(user => Downloader.UserToken = user.UserToken);
        }

        public static IObservable<UserProfileResponseContract> GetUserProfileAsync(string userId)
        {
            return Downloader.GetJson<UserProfileResponseContract>(ApiUrl.UserProfile(userId)).NotNull();
        }

        public static IObservable<MixesResponseContract> GetUserMixesAsync(string userId, int pageNumber, int pageSize)
        {
            return Downloader.GetJson<MixesResponseContract>(ApiUrl.UserMixes(userId, pageNumber, pageSize)).NotNull();
        }

        public static IObservable<MixesResponseContract> GetLikedMixesAsync(string userId, int pageNumber, int pageSize)
        {
            var cacheFile = string.Format(LikedMixesCacheFile, userId, pageNumber);
            return Downloader.GetJsonCachedAndRefreshed<MixesResponseContract>(ApiUrl.UserMixes(userId, "liked", pageNumber, pageSize), cacheFile).NotNull(); 
        }

        public static IObservable<MixesResponseContract> GetMixFeedAsync(string userId, int pageNumber, int pageSize)
        {
            var cacheFile = string.Format(MixFeedCacheFile, userId, pageNumber);
            return Downloader.GetJsonCachedAndRefreshed<MixesResponseContract>(ApiUrl.UserMixes(userId, "mix_feed", pageNumber, pageSize), cacheFile).NotNull();
        }

        public static IObservable<Unit> SetMixLikedAsync(string mixId, bool isLiked)
        {
            var urlFormat = isLiked ? string.Format("http://8tracks.com/mixes/{0}/like.json", mixId) : string.Format("http://8tracks.com/mixes/{0}/unlike.json", mixId);
            return Downloader.PostStringAndGetJson<LikedMixResponseContract>(new Uri(urlFormat, UriKind.Absolute), string.Empty).Select(r => new Unit());
        }

        public static IObservable<Unit> SetTrackFavouriteAsync(string trackId, bool isFavourite)
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

        public static IObservable<ReviewsResponseContract> GetReviewsByUserAsync(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/reviews.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetJson<ReviewsResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<FollowingUserResponseContract> GetFollowedByUsersAsync(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/followed_by_users.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetJson<FollowingUserResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<FollowingUserResponseContract> GetFollowsUsersAsync(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/follows_users.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetJson<FollowingUserResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<FollowUserResponseContract> SetFollowUserAsync(string userId, bool isFollowed)
        {
            var urlFormat = isFollowed ? 
                string.Format("http://8tracks.com/users/{0}/follow.json", userId) : 
                string.Format("http://8tracks.com/users/{0}/unfollow.json", userId);
            var url = new Uri(urlFormat, UriKind.Absolute);
            return Downloader.PostStringAndGetJson<FollowUserResponseContract>(url, string.Empty);
        }

        public static IObservable<ReviewResponseContract> AddMixReviewAsync(string mixId, string review)
        {
            var url = new Uri("http://8tracks.com/reviews", UriKind.Absolute);
            string body = string.Format("review%5Bbody%5D={0}&review%5Bmix_id%5D={1}&format=json", ApiUrl.Escape(review), mixId);
            return from response in Downloader.PostStringAndGetJson<ReviewResponseContract>(url, body)
                   from responseWithUser in GetUserProfileAsync(response.Review.UserId).Select(
                       userResponse =>
                           {
                               if (response.Review.User == null)
                               {
                                   response.Review.User = userResponse.User;
                               }

                               return response;
                           })
                   select responseWithUser;
        }

        public static IObservable<Unit> ResetAsync()
        {
            return ObservableEx.DeferredStart(DeleteCredentials);
        }
    }
}
