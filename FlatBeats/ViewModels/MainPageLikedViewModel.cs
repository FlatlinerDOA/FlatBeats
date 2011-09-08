
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel.Services;

    public class MainPageLikedViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainPageLikedViewModel class.
        /// </summary>
        public MainPageLikedViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.Title = "liked";
        }

        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public void Load()
        {
            ////this.Message = "sign in or sign up to keep track of the mixes you like.";
        }
    }
}
