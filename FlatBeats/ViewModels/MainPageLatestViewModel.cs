namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Microsoft.Phone.Reactive;

    public sealed class MainPageLatestViewModel : PanelViewModel
    {
        private readonly ProfileService profileService;

        private bool censor;

        /// <summary>
        /// Initializes a new instance of the MainPageLatestViewModel class.
        /// </summary>
        public MainPageLatestViewModel()
            : this(ProfileService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MainPageLatestViewModel class.
        /// </summary>
        public MainPageLatestViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
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
            var pageData = from _ in this.profileService.GetSettingsAsync().Do(s => this.censor = s.CensorshipEnabled)
                from latestMixes in MixesService.GetLatestMixesAsync()
                select new Page<MixContract>(latestMixes.Mixes, 1, latestMixes.Mixes.Count);
            
            return pageData.ObserveOnDispatcher().AddOrReloadPage(this.Mixes, (vm, d) => vm.Load(d, this.censor)).FinallySelect(() => (IList<MixViewModel>)this.Mixes);
        }
    }
}
