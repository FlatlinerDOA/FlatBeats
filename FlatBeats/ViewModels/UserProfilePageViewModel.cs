//--------------------------------------------------------------------------------------------------
// <copyright file="UserProfilePageViewModel.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    using FlatBeats.Controls;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class UserProfilePageViewModel : PageViewModel, IApplicationBarViewModel
    {
        /// <summary>
        /// Initializes a new instance of the UserProfilePageViewModel class.
        /// </summary>
        public UserProfilePageViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>();
        }

        public string UserId { get; set; }

        public void Load()
        {
            var mixes = from response in ProfileService.GetUserMixes(this.UserId)
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                        select new MixViewModel(mix);
            mixes.FlowIn()
                .ObserveOnDispatcher()
                .FirstDo(_ => this.Mixes.Clear())
                .Subscribe(
                    this.Mixes.Add, 
                    this.ShowError, 
                    this.HideProgress);
        }

        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }
    }
}
