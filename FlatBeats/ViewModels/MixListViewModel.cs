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
            var mixes = from response in this.DownloadTagMixes(tag).ObserveOnDispatcher().Do(_ => this.Mixes.Clear())
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool) 
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.ObserveOnDispatcher().Do(this.Mixes.Add, this.ShowError).Select(_ => new Unit());
        }

        public IObservable<Unit> Search(string searchQuery)
        {
            this.IsDataLoaded = true;
            var mixes = from response in this.DownloadSearchMixes(searchQuery).ObserveOnDispatcher().Do(_ => this.Mixes.Clear())
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
        private IObservable<MixesResponseContract> DownloadTagMixes(string tag)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?tag={0}&sort={1}", Uri.EscapeDataString(tag), this.Sort),
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
        private IObservable<MixesResponseContract> DownloadSearchMixes(string query)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?q={0}&sort={1}", Uri.EscapeDataString(query), this.Sort),
                    UriKind.RelativeOrAbsolute));
        }

    }
}
