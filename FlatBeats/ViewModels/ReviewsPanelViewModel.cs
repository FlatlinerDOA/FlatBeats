namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Microsoft.Phone.Reactive;
    using Flatliner.Phone;

    public sealed class ReviewsPanelViewModel : InfiniteScrollPanelViewModel<ReviewViewModel, ReviewContract>
    {
        private readonly ProfileService profileService;

        private bool censor;

        public ReviewsPanelViewModel() : this(ProfileService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ReviewsPanelViewModel class.
        /// </summary>
        public ReviewsPanelViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
            this.Title = StringResources.Title_Reviews;
        }

        public IObservable<Unit> LoadAsync(string mixId)
        {
            this.MixId = mixId;
            return from _ in this.profileService.GetSettingsAsync().Do(
                s => this.censor = s.CensorshipEnabled)
                   from load in this.LoadAsync()
                   select load;
        }

        public string MixId { get; private set; }

        protected override IObservable<IList<ReviewContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return MixesService.GetMixReviewsAsync(this.MixId, pageNumber, pageSize)
                .Select(r => (IList<ReviewContract>)r.Reviews);
        }

        protected override void LoadItem(ReviewViewModel viewModel, ReviewContract data)
        {
            viewModel.Load(data, this.censor);
        }

        protected override void LoadPageCompleted()
        {
            if (this.Items.Count == 0)
            {
                this.Message = StringResources.Message_NoReviews;
                this.ShowMessage = true;
            }
            else
            {
                this.Message = null;
            }
        }
    }
}
