namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MixListViewModel : PanelViewModel
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

            var mixes = from page in Observable.Range(1, 2)
                from response in MixesService.DownloadTagMixes(tag, this.Sort, page).ObserveOnDispatcher()
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool) 
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.ObserveOnDispatcher().FirstDo(_ => this.Mixes.Clear()).Do(this.Mixes.Add, this.ShowError).Select(_ => new Unit());
        }

        public IObservable<Unit> Search(string searchQuery)
        {
            this.IsDataLoaded = true;
            var mixes = from page in Observable.Range(1, 2)
                        from response in MixesService.DownloadSearchMixes(searchQuery, this.Sort, page).ObserveOnDispatcher()
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.ObserveOnDispatcher().FirstDo(_ => this.Mixes.Clear()).Do(this.Mixes.Add, this.ShowError).Select(_ => new Unit());
        }


    }
}
