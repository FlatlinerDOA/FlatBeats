
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Microsoft.Phone.Reactive;
    using FlatBeats.DataModel.Profile;
    using Flatliner.Phone;

    public sealed class MainPageLikedViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>
    {
        private string loadedList;

        /// <summary>
        /// Initializes a new instance of the MainPageLikedViewModel class.
        /// </summary>
        public MainPageLikedViewModel()
        {
            this.loadedList = UserSettings.Current.PreferredList;

            switch (this.loadedList)
            {
                case PreferredLists.Created:
                    this.Title = StringResources.Title_CreatedMixes;
                    break;
                case PreferredLists.MixFeed:
                    this.Title = StringResources.Title_MixFeed;
                    break;
                default:
                    this.Title = StringResources.Title_LikedMixes;
                    break;
            }

        }

        private string UserId { get; set; }

        public IObservable<Unit> LoadAsync(string userId) 
        {
            this.UserId = userId;
            return this.LoadAsync();
        }

        public override IObservable<Unit> LoadAsync()
        {
            if (this.loadedList != UserSettings.Current.PreferredList)
            {
                this.Reset();
            }

            this.loadedList = UserSettings.Current.PreferredList;

            switch (this.loadedList)
            {
                case PreferredLists.Created:
                    this.Title = StringResources.Title_CreatedMixes;
                    break;
                case PreferredLists.MixFeed:
                    this.Title = StringResources.Title_MixFeed;
                    break;
                default:
                    this.Title = StringResources.Title_LikedMixes;
                    break;
            }

            return base.LoadAsync().FirstDo( _ => LoadFirstPage());
        }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            if (loadedList == PreferredLists.Created)
            {
                return (from page in ProfileService.GetUserMixesAsync(this.UserId, pageNumber, pageSize)
                        select (IList<MixContract>)page.Mixes);
            }

            if (loadedList == PreferredLists.MixFeed)
            {
                return (from page in ProfileService.GetMixFeedAsync(this.UserId, pageNumber, pageSize)
                        select (IList<MixContract>)page.Mixes);
            }

            return (from page in ProfileService.GetLikedMixesAsync(this.UserId, pageNumber, pageSize)
                    select (IList<MixContract>)page.Mixes);
        }

        protected override void LoadItem(MixViewModel viewModel, MixContract data)
        {
            viewModel.Load(data);
        }

        protected override void LoadPageCompleted()
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
