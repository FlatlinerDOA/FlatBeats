﻿namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// Panel of list of users that a user is following
    /// </summary>
    public class FollowsUsersViewModel : InfiniteScrollPanelViewModel<UserListItemViewModel, UserContract>
    {
        /// <summary>
        /// Initializes a new instance of the FollowsUsersViewModel class.
        /// </summary>
        public FollowsUsersViewModel(bool isCurrentUser)
        {
            this.IsCurrentUser = isCurrentUser;
            if (this.IsCurrentUser)
            {
                this.Title = StringResources.Title_YouFollow;
            }
            else 
            {
                this.Title = StringResources.Title_FollowsUsers;
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
            return this.LoadItemsAsync();
        }

        protected bool IsCurrentUser { get; set; }

        protected string UserId { get; set; }

        protected bool IsDataLoaded { get; set; }

        protected override IObservable<IList<UserContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            return ProfileService.GetFollowsUsers(this.UserId, pageNumber, pageSize).Select(r => (IList<UserContract>)r.Users);
        }

        protected override UserListItemViewModel CreateItem(UserContract data)
        {
            return new UserListItemViewModel(data);
        }

        protected override void LoadItemsCompleted()
        {
            if (this.Items.Count == 0)
            {
                if (this.IsCurrentUser)
                {
                    this.Message = StringResources.Message_YouAreNotFollowingAnyone;
                }
                else
                {
                    this.Message = StringResources.Message_UserNotFollowingAnyone;
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
