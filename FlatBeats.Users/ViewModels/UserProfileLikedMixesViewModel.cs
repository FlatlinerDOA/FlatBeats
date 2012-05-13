
namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.Reactive;

    public class UserProfileMixesViewModel : InfiniteScrollPanelViewModel<MixViewModel, MixContract>, ILifetime<string>
    {
        private readonly ProfileService profileService;

        private bool censor;

        /// <summary>
        /// Initializes a new instance of the UserProfileMixesViewModel class.
        /// </summary>
        public UserProfileMixesViewModel(bool isCurrentUser, ProfileService profileService)
        {
            this.profileService = profileService;
            this.IsCurrentUser = isCurrentUser;
            this.Title = StringResources.Title_CreatedMixes;
        }

        public IObservable<Unit> LoadAsync(string userId)
        {
            this.UserId = userId;
            return from _ in this.profileService.GetSettingsAsync().Do(s => this.censor = s.CensorshipEnabled) 
                   from load in this.LoadAsync() 
                   select load;
        }

        protected bool IsCurrentUser { get; set; }

        protected string UserId { get; set; }

        protected override IObservable<IList<MixContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            if (this.UserId == null)
            {
                return Observable.Empty<IList<MixContract>>();
            }

            return this.profileService.GetUserMixesAsync(this.UserId, pageNumber, pageSize).Select(p => (IList<MixContract>)p.Mixes);
        }

        protected override void LoadItem(MixViewModel viewModel, MixContract data)
        {
            viewModel.Load(data, this.censor);
        }

        protected override void LoadPageCompleted()
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
