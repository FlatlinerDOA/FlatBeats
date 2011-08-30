namespace FlatBeats.ViewModels
{
    using System;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class TrackViewModel : ViewModel
    {
        /// <summary>
        /// Initializes a new instance of the TrackViewModel class.
        /// </summary>
        public TrackViewModel(TrackContract track)
        {
            this.title = track.Name;
            this.artist = track.Artist;
            this.isFavourite = track.IsFavourite;
            this.Id = track.Id;
        }

        public string Id { get; private set; }

        #region Public Properties

        private bool isFavourite;

        public bool IsFavourite
        {
            get
            {
                return this.isFavourite;
            }
            set
            {
                if (this.isFavourite == value)
                {
                    return;
                }

                this.isFavourite = value;
                this.OnPropertyChanged("IsFavourite");
                PlayerService.SetTrackFavourite(this.Id, this.isFavourite).ObserveOnDispatcher().Subscribe(
                    _ =>
                    {
                    }, 
                    ex =>
                    {
                        this.isFavourite = !this.isFavourite; 
                        this.OnPropertyChanged("IsFavourite");
                    });
            }
        }

        private string artist;

        public string Artist
        {
            get
            {
                return this.artist;
            }
            set
            {
                if (this.artist == value)
                {
                    return;
                }

                this.artist = value;
                this.OnPropertyChanged("Artist");
            }
        }

        /// <summary>
        /// </summary>
        private string title;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (this.title == value)
                {
                    return;
                }

                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        #endregion
    }
}