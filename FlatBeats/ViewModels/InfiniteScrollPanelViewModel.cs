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
        private Subject<int> pageRequests = new Subject<int>();

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
            this.pageRequests = new Subject<int>();
        }

        protected string GetLoadingPageMessage()
        {
            switch (this.CurrentPage)
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


        public void LoadFirstPage()
        {
            if (this.CurrentPage == 0)
            {
                this.LoadNextPage();
            }
        }

        public virtual void LoadNextPage()
        {
            this.CurrentPage++;
            this.pageRequests.OnNext(this.CurrentPage);
        }
    }

    public abstract class InfiniteScrollPanelViewModel<TViewModel, TData> : InfiniteScrollPanelViewModel 
        where TViewModel : class, new()
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
            this.Items.Clear();
            var getItems =
                from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
                from response in
                    this.GetPageOfItemsAsync(page, this.PageSize).Do(_ => this.HideProgress(), this.HideProgress)
                    .ContinueWhile(r => r != null && r.Count == this.PageSize, this.StopLoadingPages)
                where response != null
                select response;
            return getItems.Do(
                this.AddToList,
                this.HandleError,
                this.LoadCompleted).FinallySelect(() => new Unit());
        }

        private void LoadCompleted()
        {
            this.AddBufferedPageOfItems();
            this.LoadItemsCompleted();
        }

        public void Reset()
        {
            this.CurrentPage = 0;
            this.Items.Clear();
            this.nextPage.Clear();
        }

        protected abstract TViewModel CreateItem(TData data);

        private readonly BlockingQueue<TViewModel> nextPage = new BlockingQueue<TViewModel>();

        protected abstract void LoadItemsCompleted();

        private void AddToList(IList<TData> pageOfItems)
        {
            this.nextPage.EnqueueRange(pageOfItems.Select(this.CreateItem));
            
            if (this.Items.Count == 0)
            {
                this.AddBufferedPageOfItems();
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
            TViewModel item;
            while (this.nextPage.TryDequeue(out item))
            {
                this.Items.Add(item);
            }
        }
    }

    public sealed class BlockingQueue<T>
    {
        private readonly object syncRoot = new object();

        private readonly Queue<T> queue = new Queue<T>();

        public void Enqueue(T item)
        {
            lock (this.syncRoot)
            {
                this.queue.Enqueue(item);
            }
        }

        public void EnqueueRange(IEnumerable<T> items)
        {
            lock (this.syncRoot)
            {
                foreach (var item in items)
                {
                    this.queue.Enqueue(item);
                }
            }
        }

        public bool TryDequeue(out T item)
        {
            lock (this.syncRoot)
            {
                if (this.queue.Count == 0)
                {
                    item = default(T);
                    return false;
                }

                item = this.queue.Dequeue();
                return true;
            }
        }

        public void Clear()
        {
            lock (this.syncRoot)
            {
                this.queue.Clear();
            }   
        }
         
    }
}