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
            this.Message = null;
            var recentMixes = from response in PlayerService.RecentlyPlayedAsync()
                              let playing = PlayerService.LoadNowPlaying()
                              where response != null && response.Mixes != null
                              from mix in response.Mixes.ToObservable(Scheduler.ThreadPool).FlowIn(200).ObserveOnDispatcher()
                                  .AddOrReloadListItems(
                                      this.Mixes, 
                                      (vm, mix) =>
                                      {
                                          vm.Load(mix);
                                          vm.IsNowPlaying = playing != null && playing.MixId == mix.Id;
                                      })
                              select mix;
            return recentMixes.FinallySelect(
                    () =>
                    {
                        if (this.Mixes.Count == 0)
                        {
                            this.Message = StringResources.Message_NoRecentlyPlayedMixes;
                        }

                        return new Unit();
                    });
        }
    }
}
