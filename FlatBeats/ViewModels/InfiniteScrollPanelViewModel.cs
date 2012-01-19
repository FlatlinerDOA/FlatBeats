namespace FlatBeats.ViewModels
{
    using System;

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
        
        public int CurrentPage { get; private set; }

      
        public void StopLoadingPages()
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
}