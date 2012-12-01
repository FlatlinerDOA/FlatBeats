// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProfileService.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.DataModel.Services
{
    using System;

    using FlatBeats.DataModel.Profile;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;
    using Flatliner.Functional;

    /// <summary>
    /// </summary>
    public class ProfileService
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        public static readonly ProfileService Instance = new ProfileService();

        /// <summary>
        /// </summary>
        private const string CredentialsFilePath = "credentials.json";

        /// <summary>
        /// </summary>
        private const string LikedMixesCacheFile = "/cache/{0}/liked-{1}.json";

        /// <summary>
        /// </summary>
        private const string MixFeedCacheFile = "/cache/{0}/mixfeed-{1}.json";

        /// <summary>
        /// </summary>
        private const string SettingsFilePath = "settings.json";

        /// <summary>
        /// </summary>
        private const string UserLoginFilePath = "userlogin.json";

        /// <summary>
        /// </summary>
        private readonly IAsyncDownloader downloader;

        /// <summary>
        /// </summary>
        private readonly ISerializer<SettingsContract> settingsSerializer;

        /// <summary>
        /// </summary>
        private readonly IAsyncStorage storage;

        /// <summary>
        /// </summary>
        private SettingsContract userSettings;

        private readonly object syncRoot = new object();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public ProfileService()
            : this(AsyncIsolatedStorage.Instance, Json<SettingsContract>.Instance, AsyncDownloader.Instance)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="storage">
        /// </param>
        /// <param name="settingsSerializer">
        /// </param>
        /// <param name="downloader">
        /// </param>
        public ProfileService(
            IAsyncStorage storage, ISerializer<SettingsContract> settingsSerializer, IAsyncDownloader downloader)
        {
            this.storage = storage;
            this.downloader = downloader;
            this.settingsSerializer = settingsSerializer;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// </summary>
        /// <param name="mixId">
        /// </param>
        /// <param name="review">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<ReviewResponseContract> AddMixReviewAsync(string mixId, string review)
        {
            var url = new Uri("http://8tracks.com/reviews", UriKind.Absolute);
            string body = string.Format(
                "review%5Bbody%5D={0}&review%5Bmix_id%5D={1}&format=json", ApiUrl.Escape(review), mixId);
            return from response in this.downloader.PostStringAndGetDeserializedAsync<ReviewResponseContract>(url, body)
                   from responseWithUser in this.GetUserProfileAsync(response.Review.UserId).Select(
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

        /// <summary>
        /// </summary>
        /// <param name="userCredentials">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<UserLoginResponseContract> AuthenticateAsync(UserCredentialsContract userCredentials)
        {
            if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.UserName)
                || string.IsNullOrWhiteSpace(userCredentials.Password))
            {
                return Observable.Empty<UserLoginResponseContract>();
            }

            // downloader.UserCredentials = null;
            this.downloader.UserToken = null;
            string postData = string.Format(
                "login={0}&password={1}", 
                ApiUrl.Escape(userCredentials.UserName), 
                ApiUrl.Escape(userCredentials.Password));
            IObservable<UserLoginResponseContract> userLogin =
                from response in this.downloader.PostAndGetStringAsync(ApiUrl.Authenticate(), postData)
                let user = Json<UserLoginResponseContract>.Instance.DeserializeFromString(response)
                where !string.IsNullOrWhiteSpace(user.UserToken)
                select user;

            return (from loginResponse in userLogin
                    from _ in this.SaveCredentialsAsync(userCredentials)
                    from __ in this.SaveUserTokenAsync(loginResponse)
                    select loginResponse).Do(
                        response =>
                            {
                                PlayerService.DeletePlayToken();
                                this.downloader.UserToken = response.UserToken;

                                // downloader.UserCredentials = userCredentials;
                            });
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <param name="pageNumber">
        /// </param>
        /// <param name="pageSize">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<FollowingUserResponseContract> GetFollowedByUsersAsync(
            string userId, int pageNumber, int pageSize)
        {
            string urlFormat = string.Format(
                "http://8tracks.com/users/{0}/followed_by_users.json?page={1}&per_page={2}", 
                userId, 
                pageNumber, 
                pageSize);
            return
                this.downloader.GetDeserializedAsync<FollowingUserResponseContract>(
                    new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <param name="pageNumber">
        /// </param>
        /// <param name="pageSize">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<FollowingUserResponseContract> GetFollowsUsersAsync(
            string userId, int pageNumber, int pageSize)
        {
            string urlFormat = string.Format(
                "http://8tracks.com/users/{0}/follows_users.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return
                this.downloader.GetDeserializedAsync<FollowingUserResponseContract>(
                    new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <param name="pageNumber">
        /// </param>
        /// <param name="pageSize">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<MixesResponseContract> GetLikedMixesAsync(string userId, int pageNumber, int pageSize)
        {
            string cacheFile = string.Format(LikedMixesCacheFile, userId, pageNumber);
            return
                this.downloader.GetDeserializedCachedAndRefreshedAsync<MixesResponseContract>(
                    ApiUrl.UserMixes(userId, "liked", pageNumber, pageSize), cacheFile).NotNull();
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <param name="pageNumber">
        /// </param>
        /// <param name="pageSize">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<MixesResponseContract> GetMixFeedAsync(string userId, int pageNumber, int pageSize)
        {
            string cacheFile = string.Format(MixFeedCacheFile, userId, pageNumber);
            return
                this.downloader.GetDeserializedCachedAndRefreshedAsync<MixesResponseContract>(
                    ApiUrl.UserMixes(userId, "mix_feed", pageNumber, pageSize), cacheFile).NotNull();
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <param name="pageNumber">
        /// </param>
        /// <param name="pageSize">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<ReviewsResponseContract> GetReviewsByUserAsync(string userId, int pageNumber, int pageSize)
        {
            string urlFormat = string.Format(
                "http://8tracks.com/users/{0}/reviews.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return
                this.downloader.GetDeserializedAsync<ReviewsResponseContract>(
                    new Uri(urlFormat, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public IObservable<SettingsContract> GetSettingsAsync()
        {
            lock (this.syncRoot)
            {
                if (this.userSettings != null)
                {
                    return Observable.Return(this.userSettings);
                }
            }

            return (from settings in this.storage.LoadJsonAsync<SettingsContract>(SettingsFilePath)
                    select settings ??
                        new SettingsContract
                            {
                               CensorshipEnabled = true, 
                               PlayNextMix = true, 
                               PreferredList = PreferredLists.Liked, 
                               PlayOverWifiOnly = false 
                            }).Do(
                                s =>
                                    {
                                        lock (this.syncRoot)
                                        {
                                            this.userSettings = s;
                                        }
                                    });
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <param name="pageNumber">
        /// </param>
        /// <param name="pageSize">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<MixesResponseContract> GetUserMixesAsync(string userId, int pageNumber, int pageSize)
        {
            return this.downloader.GetDeserializedAsync<MixesResponseContract>(
                    ApiUrl.UserMixes(userId, pageNumber, pageSize)).NotNull();
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<UserProfileResponseContract> GetUserProfileAsync(string userId)
        {
            return this.downloader.GetDeserializedAsync<UserProfileResponseContract>(ApiUrl.UserProfile(userId)).NotNull();
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public IObservable<UserCredentialsContract> LoadCredentialsAsync()
        {
            return this.storage.LoadJsonAsync<UserCredentialsContract>(CredentialsFilePath).Where(c => c != null);
                
                // .Do(c => Downloader.UserCredentials = c);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public IObservable<UserLoginResponseContract> LoadUserTokenAsync()
        {
            return
                this.storage.LoadJsonAsync<UserLoginResponseContract>(UserLoginFilePath).NotNull().Where(
                    c => c.CurrentUser != null).Do(user => this.downloader.UserToken = user.UserToken);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public IObservable<PortableUnit> ResetAsync()
        {
            return ObservableEx.DeferredStart(this.DeleteCredentials);
        }

        /// <summary>
        /// </summary>
        /// <param name="settings">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<PortableUnit> SaveSettingsAsync(SettingsContract settings)
        {
            return this.storage.SaveJsonAsync(SettingsFilePath, settings).Do(
                    s =>
                        {
                            lock (this.syncRoot)
                            {
                                this.userSettings = settings;
                            }
                        });
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <param name="isFollowed">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<FollowUserResponseContract> SetFollowUserAsync(string userId, bool isFollowed)
        {
            string urlFormat = isFollowed
                                   ? string.Format("http://8tracks.com/users/{0}/follow.json", userId)
                                   : string.Format("http://8tracks.com/users/{0}/unfollow.json", userId);
            var url = new Uri(urlFormat, UriKind.Absolute);
            return this.downloader.PostStringAndGetDeserializedAsync<FollowUserResponseContract>(url, string.Empty);
        }

        /// <summary>
        /// </summary>
        /// <param name="mixId">
        /// </param>
        /// <param name="isLiked">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<PortableUnit> SetMixLikedAsync(string mixId, bool isLiked)
        {
            string urlFormat = isLiked
                                   ? string.Format("http://8tracks.com/mixes/{0}/like.json", mixId)
                                   : string.Format("http://8tracks.com/mixes/{0}/unlike.json", mixId);
            return
                this.downloader.PostStringAndGetDeserializedAsync<LikedMixResponseContract>(
                    new Uri(urlFormat, UriKind.Absolute), string.Empty).ToUnit();
        }

        /// <summary>
        /// </summary>
        /// <param name="trackId">
        /// </param>
        /// <param name="isFavourite">
        /// </param>
        /// <returns>
        /// </returns>
        public IObservable<PortableUnit> SetTrackFavouriteAsync(string trackId, bool isFavourite)
        {
            string urlFormat = isFavourite
                                   ? string.Format("http://8tracks.com/tracks/{0}/fav.json", trackId)
                                   : string.Format("http://8tracks.com/tracks/{0}/unfav.json", trackId);
            return this.downloader.PostStringAndGetDeserializedAsync<FavouritedTrackResponseContract>(new Uri(urlFormat, UriKind.Absolute), string.Empty).ToUnit();
        }

        public IObservable<FavouritedTrackListResponseContract> GetFavouriteTracksAsync(string userId, int pageNumber, int pageSize)
        {
            string urlFormat = string.Format("http://8tracks.com/users/{0}/favorite_tracks.json?page={1}&per_page={2}", userId, pageNumber, pageSize);
            return this.downloader.GetDeserializedAsync<FavouritedTrackListResponseContract>(new Uri(urlFormat, UriKind.Absolute));
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        private void DeleteCredentials()
        {
            PlayerService.StopAsync(null, TimeSpan.Zero).Subscribe();
            PlayerService.ClearRecentlyPlayed();
            PlayerService.DeletePlayToken();
            this.storage.Delete(UserLoginFilePath);
            this.storage.Delete(CredentialsFilePath);
            this.downloader.UserToken = null;

            // downloader.UserCredentials = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="userCredentials">
        /// </param>
        /// <returns>
        /// </returns>
        private IObservable<PortableUnit> SaveCredentialsAsync(UserCredentialsContract userCredentials)
        {
            return this.storage.SaveJsonAsync(CredentialsFilePath, userCredentials);
        }

        /// <summary>
        /// </summary>
        /// <param name="login">
        /// </param>
        /// <returns>
        /// </returns>
        private IObservable<PortableUnit> SaveUserTokenAsync(UserLoginResponseContract login)
        {
            return this.storage.SaveJsonAsync(UserLoginFilePath, login);
        }

        #endregion

    }
}