namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Microsoft.Phone.Reactive;

    public sealed class MixListViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>
    {
        private readonly ProfileService profileService;

        public MixListViewModel() : this(ProfileService.Instance)
        {
        }

        public MixListViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
        }

        private bool censor;

        public string Sort { get; set; }

        public string Tag { get; set; }

        public string SearchQuery { get; set; }

        public override IObservable<Unit> LoadAsync()
        {
            return from _ in this.profileService.GetSettingsAsync().Do(s => this.censor = s.CensorshipEnabled)
                   from load in base.LoadAsync()
                   select load;
        }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            if (this.Tag != null)
            {
                return MixesService.GetTagMixesAsync(this.Tag, this.Sort, pageNumber, pageSize)
                    .Select(r => (IList<MixContract>)r.Mixes);
            }

            if (this.SearchQuery != null)
            {
                return MixesService.GetSearchMixesAsync(this.SearchQuery, this.Sort, pageNumber, pageSize)
                    .Select(r => (IList<MixContract>)r.Mixes);
            }

            return Observable.Empty<IList<MixContract>>();
        }

        protected override void LoadItem(MixViewModel viewModel, MixContract data)
        {
            viewModel.Load(data, this.censor);
        }

        protected override void LoadPageCompleted()
        {
            if (this.Items.Count == 0)
            {
                this.Message = StringResources.Message_NoMixesFound;
                this.ShowMessage = true;
            }
            else
            {
                this.Message = null;
            }
        }
    }
}
