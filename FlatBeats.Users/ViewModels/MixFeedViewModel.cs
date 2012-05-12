namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Reactive;

    public class MixFeedViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>, ILifetime<string>
    {
        /// <summary>
        /// Initializes a new instance of the MixFeedViewModel class.
        /// </summary>
        public MixFeedViewModel()
        {
            this.Title = StringResources.Title_MixFeed;        
        }

        public string UserId { get; private set; }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return ProfileService.GetMixFeedAsync(this.UserId, pageNumber, pageSize).Select(r => (IList<MixContract>)r.Mixes);
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

        public IObservable<Unit> LoadAsync(string userId)
        {
            this.UserId = userId;
            return this.LoadAsync();
        }
    }
}
