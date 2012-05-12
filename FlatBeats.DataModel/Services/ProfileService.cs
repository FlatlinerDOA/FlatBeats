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
        private static readonly AsyncIsolatedStorage storage = AsyncIsolatedStorage.Instance;

        private static readonly Json<SettingsContract> settingsSerializer = Json<SettingsContract>.Instance;

        private static readonly Downloader Downloader = Downloader.Instance;

        private const string LikedMixesCacheFile = "/cache/{0}/liked-{1}.json";

        private const string MixFeedCacheFile = "/cache/{0}/mixfeed-{1}.json";

        private const string CredentialsFilePath = "credentials.json";

        private const string SettingsFilePath = "settings.json";

        private const string UserLoginFilePath = "userlogin.json";
        
        public static IObservable<SettingsContract> GetSettingsAsync()
        {
            return from json in storage.LoadStringAsync(SettingsFilePath)
                   select settingsSerializer.DeserializeFromString(json) ?? new SettingsContract() { CensorshipEnabled = true, PlayNextMix = true, PreferredList = PreferredLists.Liked };
        }

        public static IObservable<Unit> SaveSettingsAsync(SettingsContract settings)
        {
            return storage.SaveStringAsync(SettingsFilePath, settingsSerializer.SerializeToString(settings));
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
            var userLogin = from response in Downloader.PostAndGetStringAsync(ApiUrl.Authenticate(), postData)
                            let user = Json<UserLoginResponseContract>.Instance.DeserializeFromString(response)
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
            return storage.SaveJsonAsync(CredentialsFilePath, userCredentials);
        }

        public static IObservable<UserCredentialsContract> LoadCredentialsAsync()
        {
            return storage.LoadJsonAsync<UserCredentialsContract>(CredentialsFilePath).Where(c => c != null).Do(c => Downloader.UserCredentials = c);
        }

        private static IObservable<Unit> SaveUserTokenAsync(UserLoginResponseContract login)
        {
            return storage.SaveJsonAsync(UserLoginFilePath, login);
        }

        public static IObservable<UserLoginResponseContract> LoadUserTokenAsync()
        {
            return storage.LoadJsonAsync<UserLoginResponseContract>(UserLoginFilePath).NotNull()
                .Where(c => c.CurrentUser != null)
                .Do(user => Downloader.UserToken = user.UserToken);
        }

        public static IObservable<UserProfileResponseContract> GetUserProfileAsync(string userId)
        {
            return Downloader.GetDeserializedAsync<UserProfileResponseContract>(ApiUrl.UserProfile(userId)).NotNull();
        }

        public static IObservable<MixesResponseContract> GetUserMixesAsync(string userId, int pageNumber, int pageSize)
        {
            return Downloader.GetDeserializedAsync<MixesResponseContract>(ApiUrl.UserMixes(userId, pageNumber, pageSize)).NotNull();
        }

        public static IObservable<MixesResponseContract> GetLikedMixesAsync(string userId, int pageNumber, int pageSize)
        {
            var cacheFile = string.Format(LikedMixesCacheFile, userId, pageNumber);
            return Downloader.GetDeserializedCachedAndRefreshedAsync<MixesResponseContract>(ApiUrl.UserMixes(userId, "liked", pageNumber, pageSize), cacheFile).NotNull(); 
        }

        public static IObservable<MixesResponseContract> GetMixFeedAsync(string userId, int pageNumber, int pageSize)
        {
            var cacheFile = string.Format(MixFeedCacheFile, userId, pageNumber);
            return Downloader.GetDeserializedCachedAndRefreshedAsync<MixesResponseContract>(ApiUrl.UserMixes(userId, "mix_feed", pageNumber, pageSize), cacheFile).NotNull();
        }

        public static IObservable<Unit> SetMixLikedAsync(string mixId, bool isLiked)
        {
            var urlFormat = isLiked ? string.Format("http://8tracks.com/mixes/{0}/like.json", mixId) : string.Format("http://8tracks.com/mixes/{0}/unlike.json", mixId);
            return Downloader.PostStringAndGetDeserializedAsync<LikedMixResponseContract>(new Uri(urlFormat, UriKind.Absolute), string.Empty).Select(r => new Unit());
        }

        public static IObservable<Unit> SetTrackFavouriteAsync(string trackId, bool isFavourite)
        {
            var urlFormat = isFavourite ? string.Format("http://8tracks.com/tracks/{0}/fav.json", trackId) : string.Format("http://8tracks.com/tracks/{0}/unfav.json", trackId);
            return Downloader.PostStringAndGetDeserializedAsync<FavouritedTrackResponseContract>(new Uri(urlFormat, UriKind.Absolute), string.Empty).Select(r => new Unit());
        }

        private static void DeleteCredentials()
        {
            PlayerService.StopAsync(null, TimeSpan.Zero).Subscribe();
            PlayerService.ClearRecentlyPlayed();
            PlayerService.DeletePlayToken();
            storage.Delete(UserLoginFilePath);
            storage.Delete(CredentialsFilePath);
            Downloader.UserToken = null;
            Downloader.UserCredentials = null;
        }

        public static IObservable<ReviewsResponseContract> GetReviewsByUserAsync(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/reviews.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetDeserializedAsync<ReviewsResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<FollowingUserResponseContract> GetFollowedByUsersAsync(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/followed_by_users.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetDeserializedAsync<FollowingUserResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<FollowingUserResponseContract> GetFollowsUsersAsync(string userId, int pageNumber, int pageSize)
        {
            var urlFormat = string.Format("http://8tracks.com/users/{0}/follows_users.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return Downloader.GetDeserializedAsync<FollowingUserResponseContract>(new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        public static IObservable<FollowUserResponseContract> SetFollowUserAsync(string userId, bool isFollowed)
        {
            var urlFormat = isFollowed ? 
                string.Format("http://8tracks.com/users/{0}/follow.json", userId) : 
                string.Format("http://8tracks.com/users/{0}/unfollow.json", userId);
            var url = new Uri(urlFormat, UriKind.Absolute);
            return Downloader.PostStringAndGetDeserializedAsync<FollowUserResponseContract>(url, string.Empty);
        }

        public static IObservable<ReviewResponseContract> AddMixReviewAsync(string mixId, string review)
        {
            var url = new Uri("http://8tracks.com/reviews", UriKind.Absolute);
            string body = string.Format("review%5Bbody%5D={0}&review%5Bmix_id%5D={1}&format=json", ApiUrl.Escape(review), mixId);
            return from response in Downloader.PostStringAndGetDeserializedAsync<ReviewResponseContract>(url, body)
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
