namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MixListViewModel : InfiniteScrollPanelViewModel
    {
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

            var mixes = from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
                        from response in MixesService.DownloadTagMixes(tag, this.Sort, page, this.PageSize)
                            .Do(_ => this.HideProgress(), this.HideProgress)
                            .ContinueWhile(r => r.Mixes != null && r.Mixes.Count == this.PageSize, this.StopLoadingPages)
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool) 
                        select mix;
            return mixes.FlowIn().ObserveOnDispatcher().AddOrReloadListItems(this.Mixes, (vm, mix) => vm.Load(mix)).Select(_ => new Unit());
        }


        public IObservable<Unit> Search(string searchQuery)
        {
            this.IsDataLoaded = true;

            var mixes = from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
                        from response in MixesService.DownloadSearchMixes(searchQuery, this.Sort, page, this.PageSize)
                            .Do(_ => this.HideProgress(), this.HideProgress)
                            .ContinueWhile(r => r.Mixes != null && r.Mixes.Count == this.PageSize, this.StopLoadingPages)
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                        select mix;
            return mixes.FlowIn().ObserveOnDispatcher().AddOrReloadListItems(this.Mixes, (vm, mix) => vm.Load(mix)).Select(_ => new Unit());
        }
    }
}
