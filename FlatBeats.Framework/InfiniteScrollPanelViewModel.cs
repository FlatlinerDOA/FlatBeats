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
        private Subject<int> pageRequests;

        /// <summary>
        /// Initializes a new instance of the InfiniteScrollPanelViewModel class.
        /// </summary>
        public InfiniteScrollPanelViewModel()
        {
            this.PageSize = 20;
        }

        public Subject<int> PageRequests 
        {
            get
            {
                return this.pageRequests = (this.pageRequests ?? new Subject<int>()); 
            } 
        }

        public int PageSize { get; set; }
        
        public int CurrentRequestedPage { get; set; }

        public virtual void StopLoadingPages()
        {
            if (this.pageRequests != null)
            {
                this.pageRequests.OnCompleted();
                this.pageRequests = null;
            }
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
                    return StringResources.Label_LoadingMore1;
                case 4:
                    return StringResources.Label_LoadingMore2;
                case 5:
                    return StringResources.Label_LoadingMore3;
                case 6:
                    return StringResources.Label_LoadingMore4;
                case 7:
                    return StringResources.Label_LoadingMore5;
                case 8:
                    return StringResources.Label_LoadingMore6;
                case 9:
                    return StringResources.Label_LoadingMore7;
                case 10:
                    return StringResources.Label_LoadingMore8;
            }

            return StringResources.Label_LoadingMore1;
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
            this.PageRequests.OnNext(this.CurrentRequestedPage);
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
                    .ObserveOnDispatcher().AddOrReloadPage(this.Items, this.LoadItem)
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


        protected override void ShowErrorMessageOverride(Flatliner.Phone.ViewModels.ErrorMessage result)
        {
            if (this.Items.Count == 0)
            {
                base.ShowErrorMessageOverride(result);
            }
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