namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// Panel of list of users that a user is following
    /// </summary>
    public class FollowedByUsersViewModel : InfiniteScrollPanelViewModel<UserListItemViewModel, UserContract>
    {
        /// <summary>
        /// Initializes a new instance of the FollowedByUsersViewModel class.
        /// </summary>
        public FollowedByUsersViewModel(bool isCurrentUser)
        {
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
            if (this.IsDataLoaded)
            {
                return Observable.Return(new Unit());
            }

            this.IsDataLoaded = true;
            return this.LoadAsync();
        }

        protected bool IsCurrentUser { get; set; }

        protected string UserId { get; set; }

        protected bool IsDataLoaded { get; set; }

        protected override IObservable<IList<UserContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return ProfileService.GetFollowedByUsersAsync(this.UserId, pageNumber, pageSize).Select(r => (IList<UserContract>)r.Users);
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