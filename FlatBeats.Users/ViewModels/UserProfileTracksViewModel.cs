namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Reactive;

    public class UserProfileTracksViewModel: InfiniteScrollPanelViewModel<TrackViewModel, TrackContract>, ILifetime<string>
    {
        private readonly ProfileService profileService;

        private bool censor;

        /// <summary>
        /// Initializes a new instance of the UserProfileTracksViewModel class.
        /// </summary>
        public UserProfileTracksViewModel(bool isCurrentUser, ProfileService profileService)
        {
            this.profileService = profileService;
            this.IsCurrentUser = isCurrentUser;
            this.Title = StringResources.Title_FavoriteTracks;
        }

        public IObservable<Unit> LoadAsync(string userId)
        {
            this.UserId = userId;
            this.ShowProgress(StringResources.Progress_Loading);
            return from _ in this.profileService.GetSettingsAsync().Do(s => this.censor = s.CensorshipEnabled)
                   from load in this.LoadAsync()
                   select load;
        }

        protected bool IsCurrentUser { get; set; }

        protected string UserId { get; set; }

        protected override IObservable<IList<TrackContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            if (this.UserId == null)
            {
                return Observable.Empty<IList<TrackContract>>();
            }

            return this.profileService.GetFavouriteTracksAsync(this.UserId, pageNumber, pageSize).Select(p => (IList<TrackContract>)p.Tracks);
        }

        protected override void LoadItem(TrackViewModel viewModel, TrackContract data)
        {
            viewModel.Load(data, this.censor);
        }

        protected override void LoadPageCompleted()
        {
            if (this.Items.Count == 0)
            {
                if (this.IsCurrentUser)
                {
                    this.Message = StringResources.Message_YouHaveNoFavoriteTracks;
                }
                else
                {
                    this.Message = StringResources.Message_UserHasNoFavoriteTracks;
                }

                this.ShowMessage = true;
            }
            else
            {
                this.Message = null;
            }
        }
    }
}
