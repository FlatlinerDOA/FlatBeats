
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

        public IObservable<Unit> LoadAsync()
        {
            var liked = from userCredentials in ProfileService.LoadCredentials()
                        from userToken in ProfileService.LoadUserToken()
                        from response in ProfileService.GetLikedMixes(userToken.CurentUser.Id)
                        from mix in response.Mixes.ToObservable()
                        select new MixViewModel(mix);

            return liked.ObserveOnDispatcher()
                .FirstDo(_ => this.Mixes.Clear())
                .Do(
                this.Mixes.Add, 
                () =>
                {
                    if (this.Mixes.Count == 0)
                    {
                        this.Message = StringResources.Message_NoLikedMixes;
                    }
                    else
                    {
                        this.Message = null;
                    }
                }).FinallySelect(() => new Unit());
        }
    }
}
