
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class UserProfileMixesViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>
    {
        /// <summary>
        /// Initializes a new instance of the UserProfileMixesViewModel class.
        /// </summary>
        public UserProfileMixesViewModel(bool isCurrentUser)
        {
            this.IsCurrentUser = isCurrentUser;
            this.Title = StringResources.Title_CreatedMixes;
        }

        public IObservable<Unit> LoadAsync(string userId)
        {
            this.UserId = userId;
            return this.LoadItemsAsync();
        }

        protected bool IsCurrentUser { get; set; }

        protected string UserId { get; set; }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return ProfileService.GetUserMixes(this.UserId, pageNumber, pageSize).Select(p => (IList<MixContract>)p.Mixes);
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
                if (this.IsCurrentUser)
                {
                    this.Message = StringResources.Message_YouHaveNoMixes;
                }
                else
                {
                    this.Message = StringResources.Message_UserHasNoMixes;
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
