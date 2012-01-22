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
            this.Reviews = new ObservableCollection<ReviewViewModel>();
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

            return this.LoadItemsAsync();
        }

        public string MixId { get; private set; }

        public ObservableCollection<ReviewViewModel> Reviews { get; private set; }
        
        /////// <summary>
        /////// </summary>
        ////private IObservable<Unit> LoadCommentsAsync()
        ////{
        ////    this.Reviews.Clear();
        ////    var downloadComments = from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
        ////                           from response in MixesService.GetMixReviews(this.MixId, page, this.PageSize)
        ////                               .Do(_ => this.HideProgress(), this.HideProgress)
        ////                               .ContinueWhile(r => r.Reviews != null && r.Reviews.Count == this.PageSize, this.StopLoadingPages)
        ////                           where response.Reviews != null
        ////                           from review in response.Reviews.ToObservable()
        ////                           select new ReviewViewModel(review);
        ////    return downloadComments.ObserveOnDispatcher().Do(
        ////        r => this.Reviews.Add(r),
        ////        this.HandleError, 
        ////        () =>
        ////        {
        ////            if (this.Reviews.Count == 0)
        ////            {
        ////                this.Message = StringResources.Message_NoReviews;
        ////                this.ShowMessage = true;
        ////            }
        ////            else
        ////            {
        ////                this.Message = null;
        ////            }
        ////        }).FinallySelect(() => new Unit());
        ////}

        protected bool IsDataLoaded { get; set; }

        protected override IObservable<IList<ReviewContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return MixesService.GetMixReviews(this.MixId, pageNumber, pageSize)
                .Where(r => r.Reviews != null)
                .Select(r => (IList<ReviewContract>)r.Reviews);
        }

        protected override ReviewViewModel CreateItem(ReviewContract data)
        {
            return new ReviewViewModel(data);
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
