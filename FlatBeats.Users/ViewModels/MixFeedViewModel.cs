namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Reactive;

    public class MixFeedViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>, ILifetime<string>
    {
        private readonly ProfileService profileService;

        private bool censor;

        public MixFeedViewModel() : this(ProfileService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MixFeedViewModel class.
        /// </summary>
        public MixFeedViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
            this.Title = StringResources.Title_MixFeed;
        }

        public string UserId { get; private set; }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return this.profileService.GetMixFeedAsync(this.UserId, pageNumber, pageSize).Select(r => (IList<MixContract>)r.Mixes);
        }

        protected override void LoadItem(MixViewModel viewModel, MixContract data)
        {
            viewModel.Load(data, this.censor);
        }

        protected override void LoadPageCompleted()
        {
            if (this.Items.Count == 0)
            {
                this.Message = StringResources.Message_NoMixesFound;
                this.ShowMessage = true;
            }
            else
            {
                this.Message = null;
            }
        }

        public IObservable<Unit> LoadAsync(string userId)
        {
            this.UserId = userId;
            return from settings in this.profileService.GetSettingsAsync().Do(s => this.censor = s.CensorshipEnabled)
                   from load in this.LoadAsync()
                   select load;
        }
    }
}
