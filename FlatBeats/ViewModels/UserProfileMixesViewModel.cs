using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.ViewModels
{
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class UserProfileMixesViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the UserProfileMixesViewModel class.
        /// </summary>
        public UserProfileMixesViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.Title = StringResources.Title_CreatedMixes;
        }


        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public IObservable<Unit> LoadMixesAsync(string userId)
        {
            var mixes = from response in ProfileService.GetUserMixes(userId)
                        from mix in response.Mixes.ToObservable(Scheduler.ThreadPool)
                        select new MixViewModel(mix);
            return mixes.FlowIn()
                .ObserveOnDispatcher()
                .FirstDo(_ => this.Mixes.Clear())
                .Do(
                    this.Mixes.Add,
                    this.HandleError,
                    () =>
                    {
                        if (this.Mixes.Count == 0)
                        {
                            this.Message = StringResources.Message_UserHasNoMixes;
                            this.ShowMessage = true;
                        }
                        else
                        {
                            this.Message = null;
                        }
                    }).FinallySelect(() => new Unit());
        }
    }
}
