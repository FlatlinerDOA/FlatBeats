
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
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
        public static bool ForceReload = false;
        
        private readonly ProfileService profileService;

        private string loadedList;

        private bool censor;

        private string loadedUserId;

        public MainPageLikedViewModel() : this(ProfileService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MainPageLikedViewModel class.
        /// </summary>
        public MainPageLikedViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
            this.Title = " ";
        }

        private string UserId { get; set; }

        public IObservable<Unit> LoadAsync(string userId) 
        {
            this.UserId = userId;
            return this.LoadAsync();
        }

        public override IObservable<Unit> LoadAsync()
        {
            return from _ in this.profileService.GetSettingsAsync().ObserveOnDispatcher().Do(s =>
                    {
                        if (ForceReload || (this.loadedList != null && this.loadedList != s.PreferredList) || (this.UserId != this.loadedUserId) || this.censor != s.CensorshipEnabled)
                        {
                            ForceReload = false;
                            this.Reset();
                        }

                        this.censor = s.CensorshipEnabled;
                        this.loadedList = s.PreferredList;
                        this.loadedUserId = this.UserId;
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
                    })
                   from load in base.LoadAsync()
                   select load;
        }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            if (this.UserId == null)
            {
                return Observable.Return<IList<MixContract>>(new List<MixContract>());
            }

            if (this.loadedList == PreferredLists.Created)
            {
                return from page in this.profileService.GetUserMixesAsync(this.UserId, pageNumber, pageSize)
                       select (IList<MixContract>)page.Mixes;
            }

            if (this.loadedList == PreferredLists.MixFeed)
            {
                return from page in this.profileService.GetMixFeedAsync(this.UserId, pageNumber, pageSize)
                       select (IList<MixContract>)page.Mixes;
            }

            return from page in this.profileService.GetLikedMixesAsync(this.UserId, pageNumber, pageSize)
#if DEBUG
                       .Do(m => Debug.WriteLine("Liked : Page {0} of {1} actual {2}", pageNumber, pageSize, m.Mixes != null ? m.Mixes.Count : 0))
#endif
                   select (IList<MixContract>)page.Mixes;
        }

        protected override void LoadItem(MixViewModel viewModel, MixContract data)
        {
            viewModel.Load(data, this.censor);
        }

        protected override void LoadPageCompleted()
        {
            if (this.Items.Count == 0)
            {
                switch (this.loadedList)
                {
                    case PreferredLists.Created:
                        this.Message = StringResources.Message_YouHaveNoMixes;
                        break;
                    case PreferredLists.MixFeed:
                        this.Message = StringResources.Message_NoMixesInFeed;
                        break;
                    default:
                        this.Message = string.IsNullOrEmpty(this.loadedUserId) ? StringResources.Message_NoLikedMixes : StringResources.Message_YouHaveNoLikedMixes;
                        break;
                }

                this.ShowMessage = true;
            }
            else
            {
                this.Message = null;
            }
        }
    }
}
