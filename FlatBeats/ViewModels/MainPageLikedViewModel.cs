﻿
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MainPageLikedViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainPageLikedViewModel class.
        /// </summary>
        public MainPageLikedViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.Title = StringResources.Title_LikedMixes;
        }

        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public IObservable<IList<MixViewModel>> LoadAsync()
        {
            this.Message = null;
            var liked = from userCredentials in ProfileService.LoadCredentials()
                        from userToken in ProfileService.LoadUserToken()
                        from response in ProfileService.GetLikedMixes(userToken.CurentUser.Id)
                        from mix in response.Mixes.ToObservable(Scheduler.Dispatcher).AddOrReloadListItems(this.Mixes, (vm, d) => vm.Load(d))
                        select mix;
            return liked.FinallySelect(() =>
                {
                    if (this.Mixes.Count == 0)
                    {
                        this.Message = StringResources.Message_NoLikedMixes;
                    }

                    return (IList<MixViewModel>)this.Mixes;
                });
        }
    }
}
