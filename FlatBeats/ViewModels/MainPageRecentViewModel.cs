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
            var recentMixes = from response in PlayerService.RecentlyPlayedAsync()
                              let playing = PlayerService.LoadNowPlaying()
                              where response != null && response.Mixes != null
                              from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                              select new RecentMixViewModel(mix)
                                  {
                                      IsNowPlaying = playing != null && playing.MixId == mix.Id
                                  };
            return recentMixes.FlowIn()
                .ObserveOnDispatcher()
                .FirstDo(_ => this.Mixes.Clear())
                .Do(
                    this.Mixes.Add, 
                    this.HandleError, 
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
