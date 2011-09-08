namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Reactive;

    public class MainPageLatestViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainPageLatestViewModel class.
        /// </summary>
        public MainPageLatestViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.Title = "latest mixes";
        }

        public ObservableCollection<MixViewModel> Mixes { get; private set; }


        public IObservable<List<MixViewModel>> LoadAsync()
        {
            var pageData = from latest in Downloader.GetJson<MixesResponseContract>(new Uri("http://8tracks.com/mixes.json", UriKind.RelativeOrAbsolute))
                               .ObserveOnDispatcher()
                               .Do(_ => this.Mixes.Clear())
                           from mix in latest.Mixes.ToObservable(Scheduler.ThreadPool)
                           select new MixViewModel(mix);
            return pageData.ObserveOnDispatcher().Do(
                m => this.Mixes.Add(m),
                this.ShowError).Aggregate(
                    new List<MixViewModel>(), 
                    (a, m) =>
                    {
                        a.Add(m);
                        return a;
                    });
        }
    }
}
