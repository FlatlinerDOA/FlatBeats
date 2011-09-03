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
        /// <summary>
        /// </summary>
        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public string Sort { get; set; }

        public IObservable<Unit> SearchByTag(string tag)
        {
            var mixes = from response in this.DownloadTagMixes(tag).ObserveOnDispatcher().Do(_ => this.Mixes.Clear())
                        from mix in response.Mixes.ToObservable() 
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.ObserveOnDispatcher().Do(this.Mixes.Add).Select(_ => new Unit());
        }

        public IObservable<Unit> Search(string searchQuery)
        {
            this.Mixes.Clear();
            var mixes = from response in this.DownloadTagMixes(searchQuery).Do(_ => this.Mixes.Clear())
                        from mix in response.Mixes.ToObservable()
                        let mixViewModel = new MixViewModel(mix)
                        select mixViewModel;
            return mixes.ObserveOnDispatcher().Do(this.Mixes.Add).Select(_ => new Unit());
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
                    string.Format("http://8tracks.com/mixes.json?tag={0}&sort={1}", tag, this.Sort),
                    UriKind.RelativeOrAbsolute), null);
        }
    }
}
