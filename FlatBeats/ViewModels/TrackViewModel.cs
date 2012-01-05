// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.ViewModels
{
    using System;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;

    /// <summary>
    /// </summary>
    public class TrackViewModel : ViewModel, INavigationItem

    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private string artist;

        /// <summary>
        /// </summary>
        private bool isFavourite;

        /// <summary>
        /// </summary>
        private string title;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the TrackViewModel class.
        /// </summary>
        public TrackViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TrackViewModel class.
        /// </summary>
        /// <param name="track">
        /// </param>
        public TrackViewModel(TrackContract track)
        {
            this.title = track.Name;
            this.artist = track.Artist;
            this.isFavourite = track.IsFavourite;
            this.Id = track.Id;
            this.canFavouriteTrack = Downloader.IsAuthenticated;

            this.NavigationUrl = new Uri("music://zune?" + track.Artist + " " + track.Name, UriKind.Absolute);
            ////Uri url;
            ////if (Uri.TryCreate(track.BuyLink, UriKind.Absolute, out url))
            ////{
            ////    this.NavigationUrl = url;
            ////}
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
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
        public string Id { get; private set; }

        /// <summary>
        /// </summary>
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
                ProfileService.SetTrackFavourite(this.Id, this.isFavourite).ObserveOnDispatcher().Subscribe(
                    _ => { }, 
                    ex =>
                        {
                            this.isFavourite = !this.isFavourite;
                            this.OnPropertyChanged("IsFavourite");
                        });
            }
        }

        private bool canFavouriteTrack;

        public bool CanFavouriteTrack
        {
            get
            {
                return this.canFavouriteTrack;
            }
            set
            {
                if (this.canFavouriteTrack == value)
                {
                    return;
                }

                this.canFavouriteTrack = value;
                this.OnPropertyChanged("CanFavouriteTrack");
            }
        }

        /// <summary>
        /// </summary>
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

        public Uri NavigationUrl
        {
            get; set; }
    }
}