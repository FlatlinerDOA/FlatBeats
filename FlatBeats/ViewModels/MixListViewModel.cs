namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MixListViewModel : PanelViewModel, IInfiniteScroll
    {
        private int currentPage;

        private readonly Subject<int> pageRequests = new Subject<int>();

        /// <summary>
        /// Initializes a new instance of the MixListViewModel class.
        /// </summary>
        public MixListViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
        }

        public bool IsDataLoaded { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public string Sort { get; set; }

        public IObservable<Unit> SearchByTag(string tag)
        {
            this.IsDataLoaded = true;

            // TODO: Update in progress when page loads occur
            var mixes = from page in pageRequests
                        from response in MixesService.DownloadTagMixes(tag, this.Sort, currentPage, 20)
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool) 
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.FlowIn().ObserveOnDispatcher().FirstDo(_ => this.Mixes.Clear()).Do(this.Mixes.Add, this.ShowError).Select(_ => new Unit());
        }


        public IObservable<Unit> Search(string searchQuery)
        {
            this.IsDataLoaded = true;
            // TODO: Update in progress when page loads occur
            var mixes = from page in pageRequests
                        from response in MixesService.DownloadSearchMixes(searchQuery, this.Sort, currentPage, 20).ObserveOnDispatcher()
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.FlowIn().ObserveOnDispatcher().FirstDo(_ => this.Mixes.Clear()).Do(this.Mixes.Add, this.ShowError).Select(_ => new Unit());
        }

        public void LoadNextPage()
        {
            // TODO: Check if in progress
            this.currentPage++;
            this.pageRequests.OnNext(this.currentPage);
        }
    }
}
