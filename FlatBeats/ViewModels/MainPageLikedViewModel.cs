
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
            //// http://8tracks.com/users/dp/mixes.xml?view=liked
            ////Downloader.GetJson<LikedMixResponseContract>()
            ////this.Message = "sign in or sign up to keep track of the mixes you like.";
        }
    }
}
