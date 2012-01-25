﻿namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MixListViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>
    {

        public string Sort { get; set; }

        ////public IObservable<Unit> SearchByTag(string tag)
        ////{
        ////    this.IsDataLoaded = true;

        ////    var mixes = from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
        ////                from response in MixesService.DownloadTagMixes(tag, this.Sort, page, this.PageSize)
        ////                    .Do(_ => this.HideProgress(), this.HideProgress)
        ////                    .ContinueWhile(r => r.Mixes != null && r.Mixes.Count == this.PageSize, this.StopLoadingPages)
        ////                from mix in response.Mixes.ToObservable(Scheduler.ThreadPool) 
        ////                select mix;
        ////    return mixes.FlowIn().ObserveOnDispatcher().AddOrReloadListItems(this.Mixes, (vm, mix) => vm.Load(mix)).Select(_ => new Unit());
        ////}

        public string Tag { get; set; }

        public string SearchQuery { get; set; }

        ////public IObservable<Unit> Search(string searchQuery)
        ////{
        ////    this.IsDataLoaded = true;

        ////    var mixes = from page in this.PageRequests.Do(_ => this.ShowProgress(this.GetLoadingPageMessage()))
        ////                from response in MixesService.DownloadSearchMixes(searchQuery, this.Sort, page, this.PageSize)
        ////                    .Do(_ => this.HideProgress(), this.HideProgress)
        ////                    .ContinueWhile(r => r.Mixes != null && r.Mixes.Count == this.PageSize, this.StopLoadingPages)
        ////                from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
        ////                select mix;
        ////    return mixes.FlowIn().ObserveOnDispatcher().AddOrReloadListItems(this.Mixes, (vm, mix) => vm.Load(mix)).Select(_ => new Unit());
        ////}

        public IObservable<Unit> LoadAsync()
        {
            return this.LoadItemsAsync();
        }

        public bool IsDataLoaded { get; private set; }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            if (this.Tag != null)
            {
                return MixesService.DownloadTagMixes(this.Tag, this.Sort, pageNumber, pageSize).Select(r => (IList<MixContract>)r.Mixes);
            }

            return MixesService.DownloadSearchMixes(this.SearchQuery, this.Sort, pageNumber, pageSize).Select(r => (IList<MixContract>)r.Mixes);
        }

        protected override void LoadItem(MixViewModel viewModel, MixContract data)
        {
            viewModel.Load(data);
        }

        protected override MixViewModel CreateItem(MixContract data)
        {
            return new MixViewModel(data);
        }

        protected override void LoadItemsCompleted()
        {
            if (this.Items.Count == 0)
            {
                this.Message = StringResources.Message_NoMixesFound;
                this.ShowMessage = true;
            }
            else
            {
                this.Message = null;
            }
        }
    }
}
