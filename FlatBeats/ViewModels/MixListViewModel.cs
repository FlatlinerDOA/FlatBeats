namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Microsoft.Phone.Reactive;

    public class MixListViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>
    {
        public string Sort { get; set; }

        public string Tag { get; set; }

        public string SearchQuery { get; set; }

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
            viewModel.Load(data);
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
