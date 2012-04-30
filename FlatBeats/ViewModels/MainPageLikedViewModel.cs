
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

    public class MainPageLikedViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>
    {
        /// <summary>
        /// Initializes a new instance of the MainPageLikedViewModel class.
        /// </summary>
        public MainPageLikedViewModel()
        {
        }

        private string loadedList;

        public override IObservable<Unit> LoadAsync()
        {
            ////if (this.IsDataLoaded)
            ////{
            ////    this.Reset();
            ////}

            if (loadedList != UserSettings.Current.PreferredList)
            {
                loadedList = UserSettings.Current.PreferredList;

                switch (loadedList)
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

                this.CurrentRequestedPage = 0;
                this.Items.Clear();
            }

            return from userToken in ProfileService.LoadUserTokenAsync().Do(u => this.UserId = u.CurrentUser.Id)
                   from result in base.LoadAsync()
                   select result;
        }

        protected string UserId { get; set; }

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
