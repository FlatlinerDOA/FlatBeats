namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Reactive;

    using Flatliner.Phone;

    /// <summary>
    /// Panel of list of users that a user is following
    /// </summary>
    public class FollowedByUsersViewModel : InfiniteScrollPanelViewModel<UserListItemViewModel, UserContract>, ILifetime<string>
    {
        private readonly ProfileService profileService;

        /// <summary>
        /// Initializes a new instance of the FollowedByUsersViewModel class.
        /// </summary>
        public FollowedByUsersViewModel(bool isCurrentUser, ProfileService profileService)
        {
            this.profileService = profileService;
            this.IsCurrentUser = isCurrentUser;
            if (this.IsCurrentUser)
            {
                this.Title = StringResources.Title_YouAreFollowedBy;
            }
            else
            {
                this.Title = StringResources.Title_FollowedByUsers;
            }
        }

        /// <summary>
        /// Loads the lsit of users that this person is following
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IObservable<Unit> LoadAsync(string userId)
        {
            this.UserId = userId;
            return this.LoadAsync();
        }

        protected bool IsCurrentUser { get; set; }

        protected string UserId { get; set; }


        protected override IObservable<IList<UserContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return this.profileService.GetFollowedByUsersAsync(this.UserId, pageNumber, pageSize).Select(r => (IList<UserContract>)r.Users);
        }

        protected override void LoadItem(UserListItemViewModel viewModel, UserContract data)
        {
            viewModel.Load(data);
        }

        protected override void LoadPageCompleted()
        {
            if (this.Items.Count == 0)
            {
                if (this.IsCurrentUser)
                {
                    this.Message = StringResources.Message_YouAreNotFollowedByAnyone;
                }
                else
                {
                    this.Message = StringResources.Message_UserNotFollowedByAnyone;
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