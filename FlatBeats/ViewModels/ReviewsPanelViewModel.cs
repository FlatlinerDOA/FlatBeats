namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class ReviewsPanelViewModel : InfiniteScrollPanelViewModel
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

            return this.LoadCommentsAsync();
        }

        public string MixId { get; private set; }

        public ObservableCollection<ReviewViewModel> Reviews { get; private set; }
        
        /// <summary>
        /// </summary>
        private IObservable<Unit> LoadCommentsAsync()
        {
            this.Reviews.Clear();
            var downloadComments = from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
                                   from response in MixesService.GetMixReviews(this.MixId, page, this.PageSize)
                                       .Finally(this.HideProgress)
                                       .ContinueWhile(r => r.Reviews != null && r.Reviews.Count == this.PageSize, this.StopLoadingPages)
                                   where response.Reviews != null
                                   from review in response.Reviews.ToObservable()
                                   select new ReviewViewModel(review);
            return downloadComments.ObserveOnDispatcher().Do(
                r => this.Reviews.Add(r),
                this.HandleError, 
                () =>
                {
                    if (this.Reviews.Count == 0)
                    {
                        this.Message = StringResources.Message_NoReviews;
                        this.ShowMessage = true;
                    }
                    else
                    {
                        this.Message = null;
                    }
                }).FinallySelect(() => new Unit());
        }

        protected bool IsDataLoaded { get; set; }
    }
}
