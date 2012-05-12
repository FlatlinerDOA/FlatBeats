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
        /// <summary>
        /// Initializes a new instance of the ReviewsPanelViewModel class.
        /// </summary>
        public ReviewsPanelViewModel()
        {
            this.Title = Framework.StringResources.Title_Reviews;
        }

        public IObservable<Unit> LoadAsync(string mixId)
        {
            this.MixId = mixId;
            return this.LoadAsync();
        }

        public string MixId { get; private set; }

        protected override IObservable<IList<ReviewContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return MixesService.GetMixReviewsAsync(this.MixId, pageNumber, pageSize)
                .Where(r => r.Reviews != null)
                .Select(r => (IList<ReviewContract>)r.Reviews);
        }

        protected override void LoadItem(ReviewViewModel viewModel, ReviewContract data)
        {
            viewModel.Load(data);
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
