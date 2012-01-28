namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using FlatBeats.Framework;

    using Flatliner.Phone.Data;

    using Microsoft.Phone.Reactive;

    public class InfiniteScrollPanelViewModel : PanelViewModel, IInfiniteScroll
    {
        private Subject<int> pageRequests = new Subject<int>();

        /// <summary>
        /// Initializes a new instance of the InfiniteScrollPanelViewModel class.
        /// </summary>
        public InfiniteScrollPanelViewModel()
        {
            this.PageSize = 10;
        }

        public IObservable<int> PageRequests 
        {
            get
            {
                return this.pageRequests; 
            } 
        }

        public int PageSize { get; set; }
        
        public int CurrentRequestedPage { get; set; }

        public virtual void StopLoadingPages()
        {
            this.pageRequests.OnCompleted();
            this.pageRequests = new Subject<int>();
        }

        public override void Unload()
        {
            this.StopLoadingPages();
            base.Unload();
        }

        protected string GetLoadingPageMessage()
        {
            switch (this.CurrentRequestedPage)
            {
                case 0:
                case 1:
                    return StringResources.Progress_Loading;
                case 2:
                case 3:
                    return "Loading more...";
                case 4:
                    return "Loading even more...";
                case 5:
                    return "Loading even more still...";
                case 6:
                    return "There's a lot isn't there?";
                case 7:
                    return "Have you found what you're looking for?";
                case 8:
                    return "Any minute now...";
                case 9:
                    return "I'd hate to scroll to the top right now...";
                case 10:
                    return "You just let me know when you're done...";
            }

            return "Loading more...";
        }


        public virtual void LoadFirstPage()
        {
            if (this.CurrentRequestedPage == 0)
            {
                this.LoadNextPage();
            }
        }

        public virtual void LoadNextPage()
        {
            this.CurrentRequestedPage++;
            this.pageRequests.OnNext(this.CurrentRequestedPage);
        }
    }

    public abstract class InfiniteScrollPanelViewModel<TViewModel, TData> : InfiniteScrollPanelViewModel 
        where TViewModel : ListItemViewModel, new()
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

        public virtual IObservable<Unit> LoadAsync()
        {
            return this.LoadItemsAsync();
        }

        protected IObservable<Unit> LoadItemsAsync()
        {
            ////this.CurrentRequestedPage = 0;
            var getItems =
                from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
                from response in this.GetPageOfItemsAsync(page, this.PageSize)
                    .Select(p => new Page<TData>(p, page, this.PageSize))
                    .AddOrReloadPage(this.Items, this.LoadItem)
                    .Do(
                    _ =>
                    {
                        this.HideProgress();
                        this.LoadPageCompleted();
                    }, 
                    () => 
                    {
                        this.HideProgress();
                        this.LoadPageCompleted();
                    })
                    .ContinueWhile(r => r != null && r.Count == this.PageSize, this.StopLoadingPages)
                where response != null
                select response;
            return getItems.Do(
                    _ =>
                    {
                    },
                    this.HandleError,
                    this.LoadCompleted).FinallySelect(() => new Unit());
        }

        protected abstract void LoadItem(TViewModel viewModel, TData data);

        private void LoadCompleted()
        {
            this.HideProgress();
            this.LoadPageCompleted();
        }

        public void Reset()
        {
            this.CurrentRequestedPage = 0;
            this.Items.Clear();
            this.LoadPageCompleted();
        }

        protected abstract void LoadPageCompleted();

        public override void LoadFirstPage()
        {
            this.CurrentRequestedPage = 0;
            base.LoadFirstPage();
        }
    }
}