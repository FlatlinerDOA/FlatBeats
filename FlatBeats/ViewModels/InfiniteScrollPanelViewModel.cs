namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Flatliner.Phone.Data;

    using Microsoft.Phone.Reactive;

    public class InfiniteScrollPanelViewModel : PanelViewModel, IInfiniteScroll
    {
        private readonly Subject<int> pageRequests = new Subject<int>();

        /// <summary>
        /// Initializes a new instance of the InfiniteScrollPanelViewModel class.
        /// </summary>
        public InfiniteScrollPanelViewModel()
        {
            this.PageSize = 20;
        }

        public IObservable<int> PageRequests 
        {
            get
            {
                return this.pageRequests; 
            } 
        }

        public int PageSize { get; set; }
        
        public int CurrentPage { get; set; }

        public virtual void StopLoadingPages()
        {
            this.pageRequests.OnCompleted();
        }

        protected string GetLoadingPageMessage()
        {
            switch (this.CurrentPage)
            {
                case 0:
                case 1:
                    return StringResources.Progress_Loading;
                case 2:
                    return "Loading more...";
                case 3:
                    return "Loading even more...";
                case 4:
                    return "Loading even more still...";
                case 5:
                    return "There's a lot isn't there?";
                case 6:
                    return "Have you found what you're looking for?";
                case 7:
                    return "Any minute now...";
                case 8:
                    return "You just let me know when you're done...";
            }

            return "Loading more...";
        }

        public virtual void LoadNextPage()
        {
            this.CurrentPage++;
            this.pageRequests.OnNext(CurrentPage);
        }
    }

    public abstract class InfiniteScrollPanelViewModel<TViewModel, TData> : InfiniteScrollPanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the InfiniteScrollPanelViewModel class.
        /// </summary>
        public InfiniteScrollPanelViewModel()
        {
            this.Items = new ObservableCollection<TViewModel>();
        }

        public ObservableCollection<TViewModel> Items { get; private set; }

        protected abstract IObservable<IList<TData>> GetPageOfItemsAsync(int pageNumber, int pageSize);

        protected IObservable<Unit> LoadItemsAsync()
        {
            this.CurrentPage = 0;
            this.Items.Clear();
            var getItems =
                from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
                from response in
                    this.GetPageOfItemsAsync(page, this.PageSize).Do(_ => this.HideProgress(), this.HideProgress)
                    .ContinueWhile(r => r != null && r.Count == this.PageSize, this.StopLoadingPages)
                where response != null
                select response;
            return getItems.ObserveOnDispatcher().Do(
                this.AddToList,
                this.HandleError,
                this.LoadCompleted).FinallySelect(() => new Unit());
        }

        private void LoadCompleted()
        {
            this.AddBufferedPageOfItems();
            this.LoadItemsCompleted();
        }

        protected abstract TViewModel CreateItem(TData data);

        private readonly List<TViewModel> nextPage = new List<TViewModel>();

        protected abstract void LoadItemsCompleted();

        private void AddToList(IList<TData> pageOfItems)
        {
            if (this.Items.Count == 0)
            {
                this.Items.AddAll(pageOfItems.Select(this.CreateItem));
                base.LoadNextPage();
            }
            else
            {
                this.nextPage.AddRange(pageOfItems.Select(this.CreateItem));
            }
        }

        public override void StopLoadingPages()
        {
            this.AddBufferedPageOfItems();
            base.StopLoadingPages();
        }

        public override void LoadNextPage()
        {
            this.AddBufferedPageOfItems();
            base.LoadNextPage();
        }

        private void AddBufferedPageOfItems()
        {
            if (this.nextPage.Count != 0)
            {
                this.Items.AddAll(this.nextPage);
                this.nextPage.Clear();
            }
        }
    }
}