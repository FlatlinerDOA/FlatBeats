namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Microsoft.Phone.Reactive;

    public sealed class MainPageRecentViewModel : PanelViewModel
    {
        private readonly ProfileService profileService;

        private bool censor;

        public MainPageRecentViewModel() : this(ProfileService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MainPageRecentViewModel class.
        /// </summary>
        public MainPageRecentViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
            this.Mixes = new ObservableCollection<RecentMixViewModel>();
            this.Title = StringResources.Title_RecentlyPlayedMixes;
        }

        public ObservableCollection<RecentMixViewModel> Mixes { get; private set; }

        public IObservable<Unit> LoadAsync()
        {
            this.Message = null;
            var recentMixes = from settings in this.profileService.GetSettingsAsync().Do(s => this.censor = s.CensorshipEnabled)
                              from playing in PlayerService.LoadNowPlayingAsync()
                              from recentlyPlayed in PlayerService.RecentlyPlayedAsync()
                                  .Select(p => new Page<MixContract>(p.Mixes, 1, p.Mixes.Count)).Do(_ => Debug.WriteLine("Load recent list"))
                                  .ObserveOnDispatcher().AddOrReloadPage(
                                      this.Mixes,
                                      (vm, mix) =>
                                      {
                                          vm.Load(mix, this.censor);
                                          vm.IsNowPlaying = playing != null && playing.MixId == mix.Id;
                                      }) 
                              select recentlyPlayed;
            return recentMixes.FinallySelect(
                    () =>
                    {
                        if (this.Mixes.Count == 0)
                        {
                            this.Message = StringResources.Message_NoRecentlyPlayedMixes;
                            this.ShowMessage = true;
                        }

                        return new Unit();
                    });
        }
    }
}
