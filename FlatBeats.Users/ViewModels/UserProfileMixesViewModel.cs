
namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Reactive;

    public class UserProfileLikedMixesViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>
    {
        /// <summary>
        /// Initializes a new instance of the UserProfileMixesViewModel class.
        /// </summary>
        public UserProfileLikedMixesViewModel(bool isCurrentUser)
        {
            this.IsCurrentUser = isCurrentUser;
            this.Title = StringResources.Title_LikedMixes;
        }

        public IObservable<Unit> LoadAsync(string userId)
        {
            this.UserId = userId;
            return this.LoadAsync();
        }

        protected bool IsCurrentUser { get; set; }

        protected string UserId { get; set; }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            if (this.UserId == null)
            {
                return Observable.Empty<IList<MixContract>>();
            }

            return ProfileService.GetLikedMixesAsync(this.UserId, pageNumber, pageSize).Select(p => (IList<MixContract>)p.Mixes);
        }

        protected override void LoadItem(MixViewModel viewModel, MixContract data)
        {
            viewModel.Load(data);
        }

        protected override void LoadPageCompleted()
        {
            if (this.Items.Count == 0)
            {
                if (this.IsCurrentUser)
                {
                    this.Message = StringResources.Message_YouHaveNoLikedMixes;
                }
                else
                {
                    this.Message = StringResources.Message_UserHasNoLikedMixes;
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
