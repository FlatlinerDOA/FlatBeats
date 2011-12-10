
namespace FlatBeats.ViewModels
{
    using System;
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

        public IObservable<Unit> LoadAsync()
        {
            var liked = from userCredentials in ProfileService.LoadCredentials()
                        from userToken in ProfileService.LoadUserToken()
                        from response in ProfileService.GetLikedMixes(userToken.CurentUser.Id)
                        from mix in response.Mixes.ToObservable(Scheduler.Dispatcher).AddOrReloadByPosition(this.Mixes, (vm, d) => vm.Load(d))
                        select mix;
            return liked.FinallySelect(() => new Unit());
            ////return liked.FlowIn().ObserveOnDispatcher()
            ////    .FirstDo(_ => this.Mixes.Clear())
            ////    .Do(
            ////    this.Mixes.Add, 
            ////    () =>
            ////    {
            ////        if (this.Mixes.Count == 0)
            ////        {
            ////            this.Message = StringResources.Message_NoLikedMixes;
            ////        }
            ////        else
            ////        {
            ////            this.Mixes.SetLastItem();
            ////            this.Message = null;
            ////        }
            ////    }).FinallySelect(() => new Unit());
        }
    }
}
