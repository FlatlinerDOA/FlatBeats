namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MainPageRecentViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainPageRecentViewModel class.
        /// </summary>
        public MainPageRecentViewModel()
        {
            this.Mixes = new ObservableCollection<RecentMixViewModel>();
            this.Title = StringResources.Title_RecentlyPlayedMixes;
        }

        public ObservableCollection<RecentMixViewModel> Mixes { get; private set; }

        public IObservable<Unit> LoadAsync()
        {
            ////var nowPlaying = from playingMix in Observable.Start(() => PlayerService.LoadNowPlaying())
            ////                 where playingMix != null
            ////                 select new RecentMixViewModel()
            ////                         {
            ////                             IsNowPlaying = true,
            ////                             MixId = playingMix.MixId,
            ////                             MixName = playingMix.MixName,
            ////                             TileTitle = playingMix.MixName,
            ////                             ImageUrl = playingMix.Cover.OriginalUrl,
            ////                             ThumbnailUrl = playingMix.Cover.ThumbnailUrl,
            ////                             NavigationUrl = new Uri("/PlayPage.xaml?mix=" + playingMix.MixId, UriKind.Relative)
            ////                         };
            var recentMixes = from response in PlayerService.RecentlyPlayedAsync()
                              let playing = PlayerService.LoadNowPlaying()
                              where response != null && response.Mixes != null
                              from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                              select new RecentMixViewModel(mix) { IsNowPlaying = playing != null && playing.MixId == mix.Id };
            return recentMixes.FlowIn()
                .ObserveOnDispatcher()
                .FirstDo(_ => this.Mixes.Clear())
                .Do(
                    this.Mixes.Add, 
                    this.ShowError, 
                    () =>
                    {
                        if (this.Mixes.Count == 0)
                        {
                            this.Message = StringResources.Message_NoRecentlyPlayedMixes;
                        }
                        else
                        {
                            this.Mixes.SetLastItem();
                            this.Message = null;
                        }
                    }).FinallySelect(() => new Unit());
        }
    }
}
