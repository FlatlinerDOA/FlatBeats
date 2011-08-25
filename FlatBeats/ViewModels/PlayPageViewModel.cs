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

namespace EightTracks.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;

    using EightTracks.DataModel;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Reactive;

    public class PlayPageViewModel : ViewModel
    {
        /// <summary>
        /// Initializes a new instance of the PlayPageViewModel class.
        /// </summary>
        public PlayPageViewModel()
        {
            this.Player = BackgroundAudioPlayer.Instance;
            this.Player.PlayStateChanged += this.PlayStateChanged;
        }

        private void PlayStateChanged(object sender, EventArgs e)
        {
        }

        public BackgroundAudioPlayer Player { get; set; }

        public void Load()
        {
            Downloader.DownloadJson<SingleMixContract>(new Uri(string.Format("http://8tracks.com/mixes/{0}.json", this.MixId), UriKind.RelativeOrAbsolute), "playingmix.json")
                .ObserveOnDispatcher()
                .Subscribe(mixes => this.LoadMix(mixes.Mix));
        }

        public string MixId { get; set; }

        private TrackViewModel currentTrack;

        public TrackViewModel CurrentTrack
        {
            get
            {
                return this.currentTrack;
            }
            set
            {
                if (this.currentTrack == value)
                {
                    return;
                }

                this.currentTrack = value;
                this.OnPropertyChanged("CurrentTrack");
            }
        }

        public ObservableCollection<TrackViewModel> PlayedTracks { get; private set; }

        private void LoadMix(MixContract mix)
        {
            this.MixName = mix.Name;
            
            ////this.Player.Track = new AudioTrack(this.CurrentTrack.AudioUrl, this.CurrentTrack.Title, this.MixName, "", new Uri());

        }

        private string mixName;

        public string MixName
        {
            get
            {
                return this.mixName;
            }
            set
            {
                if (this.mixName == value)
                {
                    return;
                }

                this.mixName = value;
                this.OnPropertyChanged("MixName");
            }
        }

        public void LoadPlaying()
        {
            
        }
    }

    public class TrackViewModel : ViewModel
    {
        public Uri AudioUrl { get; private set; }

        public string Title { get; private set; }
    }
}
