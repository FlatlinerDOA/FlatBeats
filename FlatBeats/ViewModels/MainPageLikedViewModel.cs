
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MainPageLikedViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>
    {
        /// <summary>
        /// Initializes a new instance of the MainPageLikedViewModel class.
        /// </summary>
        public MainPageLikedViewModel()
        {
            this.Title = StringResources.Title_LikedMixes;
        }

        public bool IsDataLoaded { get; set; }

        public IObservable<Unit> LoadAsync()
        {
            if (this.IsDataLoaded)
            {
                this.Reset();
                return this.LoadItemsAsync();
            }

            this.IsDataLoaded = true;

            var load = from userCredentials in ProfileService.LoadCredentials()
                       from userToken in ProfileService.LoadUserToken().Do(u => this.UserId = u.CurrentUser.Id)
                       from items in this.LoadItemsAsync() 
                       select new Unit();
            return load.Do(_ => { }, this.LoadItemsCompleted).FinallySelect(() => new Unit());
        }

        protected string UserId { get; set; }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return ProfileService.GetLikedMixes(this.UserId, pageNumber, pageSize).Select(t => (IList<MixContract>)t.Mixes);
        }

        protected override void LoadItem(MixViewModel viewModel, MixContract data)
        {
            viewModel.Load(data);
        }

        protected override MixViewModel CreateItem(MixContract data)
        {
            return new MixViewModel(data);
        }

        protected override void LoadItemsCompleted()
        {
            if (this.Items.Count == 0)
            {
                this.Message = StringResources.Message_NoLikedMixes;
                this.ShowMessage = true;
            }
            else
            {
                this.Message = null;
            }

        }
    }
}
