namespace FlatBeats.ViewModels
{
    using System;
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

        private readonly Subject<Unit> loaded = new Subject<Unit>();
        
        public IObservable<Unit> Loaded 
        { 
            get
            {
                return this.loaded;
            } 
        }

        public void Load()
        {
            var pageData = from latest in Downloader.GetJson<MixesResponseContract>(new Uri("http://8tracks.com/mixes.json", UriKind.RelativeOrAbsolute)).ObserveOnDispatcher().Do(_ => this.Mixes.Clear())
                           from mix in latest.Mixes.ToObservable(Scheduler.ThreadPool)
                           select new MixViewModel(mix);
            pageData.ObserveOnDispatcher().Subscribe(
                m => this.Mixes.Add(m),
                this.ShowError, () => this.loaded.OnNext(new Unit()));
        }
    }
}
