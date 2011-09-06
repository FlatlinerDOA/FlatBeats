namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;

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
            this.Mixes.Clear();
            var mixes = from page in Observable.Range(1, 2)
                from response in this.DownloadTagMixes(tag, page).ObserveOnDispatcher()
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool) 
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.ObserveOnDispatcher().Do(this.Mixes.Add, this.ShowError).Select(_ => new Unit());
        }

        public IObservable<Unit> Search(string searchQuery)
        {
            this.IsDataLoaded = true;
            this.Mixes.Clear();
            var mixes = from page in Observable.Range(1, 2)
                        from response in this.DownloadSearchMixes(searchQuery, page).ObserveOnDispatcher()
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.ObserveOnDispatcher().Do(this.Mixes.Add, this.ShowError).Select(_ => new Unit());
        }

        /// <summary>
        /// </summary>
        /// <param name="tag">
        /// </param>
        /// <param name="sort">
        /// </param>
        /// <returns>
        /// </returns>
        private IObservable<MixesResponseContract> DownloadTagMixes(string tag, int pageNumber)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?tag={0}&sort={1}&page={2}", Uri.EscapeDataString(tag), this.Sort, pageNumber),
                    UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// </summary>
        /// <param name="tag">
        /// </param>
        /// <param name="sort">
        /// </param>
        /// <returns>
        /// </returns>
        private IObservable<MixesResponseContract> DownloadSearchMixes(string query, int pageNumber)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?q={0}&sort={1}&page={2}", Uri.EscapeDataString(query), this.Sort, pageNumber),
                    UriKind.RelativeOrAbsolute));
        }

    }
}
