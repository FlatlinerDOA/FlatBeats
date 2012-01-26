
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

        public override IObservable<Unit> LoadAsync()
        {
            ////if (this.IsDataLoaded)
            ////{
            ////    this.Reset();
            ////}

            this.IsDataLoaded = true;
            this.UserId = null;
            var load = from userToken in ProfileService.LoadUserToken().Do(u => this.UserId = u.CurrentUser.Id)
                       from items in this.LoadItemsAsync() 
                       select new Unit();
            return load.Do(_ => { }, this.LoadPageCompleted).FinallySelect(() => new Unit());
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

        protected override void LoadPageCompleted()
        {
            if (this.UserId == null || this.Items.Count == 0)
            {
                this.Items.Clear();
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
