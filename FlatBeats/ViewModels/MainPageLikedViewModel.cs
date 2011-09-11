
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

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

        public void Load()
        {
            var liked = from userCredentials in UserService.LoadCredentials()
                        from mixes in UserService.GetLikedMixes()
                        select mixes;

            liked.ObserveOnDispatcher().Subscribe(
                _ => { }, 
                () =>
                {
                    if (this.Mixes.Count == 0)
                    {
                        this.Message = StringResources.Message_NoLikedMixes;
                    }
                });
        }
    }
}
