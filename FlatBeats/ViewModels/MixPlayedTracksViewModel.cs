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

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MixPlayedTracksViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MixPlayedTracksViewModel class.
        /// </summary>
        public MixPlayedTracksViewModel()
        {
            this.Tracks = new ObservableCollection<TrackViewModel>();
        }

        /// <summary>
        /// </summary>
        public ObservableCollection<TrackViewModel> Tracks { get; private set; }


        public IObservable<Unit> LoadAsync(MixContract mixData)
        {
            this.Tracks.Clear();
            var tracks = from response in mixData.PlayedTracks()
                         where response.Tracks != null
                         from track in response.Tracks.ToObservable()
                         select new TrackViewModel(track);
            return tracks.ObserveOnDispatcher().Do(
                    t => this.Tracks.Add(t), 
                    () =>
                    {
                        if (this.Tracks.Count == 0)
                        {
                            this.Message = "No tracks have been played recently for this mix.";
                            this.ShowMessage = true;
                        }
                        else
                        {
                            this.Message = null;
                        }
                    }).Select(_ => new Unit()).Catch<Unit, Exception>(
                        ex => 
                        { 
                            this.ShowError(ex);
                            return Observable.Return(new Unit());
                        });
        }
    }
}
