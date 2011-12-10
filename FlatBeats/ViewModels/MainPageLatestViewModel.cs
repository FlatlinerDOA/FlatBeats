namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MainPageLatestViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainPageLatestViewModel class.
        /// </summary>
        public MainPageLatestViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
            for (int i = 0; i < 12; i++)
            {
                this.Mixes.Add(new MixViewModel());
            }

            this.Title = StringResources.Title_LatestMixes;
        }

        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public IObservable<IList<MixViewModel>> LoadAsync()
        {
            var pageData = from latest in MixesService.GetLatestMixes()
                           from mix in latest.Mixes.ToObservable(Scheduler.Dispatcher).AddOrReloadByPosition(this.Mixes, (vm, d) => vm.Load(d))
                           select mix;
            return pageData.FinallySelect(() => (IList<MixViewModel>)this.Mixes);
            ////return pageData.FlowIn(200)
            ////    .ObserveOnDispatcher()
            ////    .FirstDo(_ => this.Mixes.Clear())
            ////    .Do(m => this.Mixes.Add(m), this.ShowError).Aggregate(
            ////        new List<MixViewModel>(), 
            ////        (a, m) =>
            ////        {
            ////            a.Add(m);
            ////            return a;
            ////        });
        }
    }
}
