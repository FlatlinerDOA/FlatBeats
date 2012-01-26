namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class ReviewsPanelViewModel : InfiniteScrollPanelViewModel<ReviewViewModel, ReviewContract>
    {
        /// <summary>
        /// Initializes a new instance of the ReviewsPanelViewModel class.
        /// </summary>
        public ReviewsPanelViewModel()
        {
            this.Title = StringResources.Title_Reviews;
        }

        public IObservable<Unit> LoadAsync(string mixId)
        {
            this.MixId = mixId;

            if (this.IsDataLoaded)
            {
                return Observable.Return(new Unit());
            }

            this.IsDataLoaded = true;

            return this.LoadAsync();
        }

        public string MixId { get; private set; }

        protected bool IsDataLoaded { get; set; }

        protected override IObservable<IList<ReviewContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return MixesService.GetMixReviews(this.MixId, pageNumber, pageSize)
                .Where(r => r.Reviews != null)
                .Select(r => (IList<ReviewContract>)r.Reviews);
        }

        protected override void LoadItem(ReviewViewModel viewModel, ReviewContract data)
        {
            viewModel.Load(data);
        }

        protected override void LoadItemsCompleted()
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
