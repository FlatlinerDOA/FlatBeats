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
    using System.Text.RegularExpressions;
    using System.Windows.Input;

    using FlatBeats.Controls;
    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Flatliner.Phone;
    using Flatliner.Phone.Data;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;

    /// <summary>
    /// </summary>
    public class TrackViewModel : ViewModel, INavigationItem
    {
        #region Constants and Fields

        private static readonly Uri AddToFavouritesIcon = new Uri("/icons/appbar.favs.addto.rest.png", UriKind.Relative);

        private static readonly Uri RemoveFromFavouritesIcon = new Uri("/icons/appbar.favs.removefrom.rest.png", UriKind.Relative);

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
            this.ToggleFavouriteCommand = new CommandLink()
                {
                    Command = new DelegateCommand(
                        this.ToggleFavourite, 
                        () => Downloader.IsAuthenticated), 
                    IconUrl = AddToFavouritesIcon, 
                    HideWhenInactive = !IsInDesignMode
                };
        }

        /// <summary>
        /// Initializes a new instance of the TrackViewModel class.
        /// </summary>
        /// <param name="track">
        /// </param>
        public TrackViewModel(TrackContract track) : this()
        {
            this.title = track.Name;
            this.artist = track.Artist;
            this.Id = track.Id;
            const string Pattern = @"\((.|\n)*?\)";

            var simplifiedArtistName = Regex.Replace(track.Artist, Pattern, string.Empty);
            var simplifiedTrackName = Regex.Replace(track.Name, Pattern, string.Empty);
            this.NavigationUrl = new Uri("music://zune?" + simplifiedArtistName + " " + simplifiedTrackName, UriKind.Absolute);
            this.IsFavourite = track.IsFavourite;
        }

        private void ToggleFavourite()
        {
            this.IsFavourite = !this.IsFavourite;
            ProfileService.SetTrackFavourite(this.Id, this.IsFavourite).ObserveOnDispatcher().Subscribe(
                _ => { },
                ex => this.IsFavourite = !this.IsFavourite);
        }

        private void UpdateFavouriteState()
        {
            if (this.IsFavourite)
            {
                this.ToggleFavouriteCommand.IconUrl = RemoveFromFavouritesIcon;
            }
            else
            {
                this.ToggleFavouriteCommand.IconUrl = AddToFavouritesIcon;
            }

            this.ToggleFavouriteCommand.RaiseCanExecuteChanged();

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
                this.UpdateFavouriteState();

            }
        }

        public CommandLink ToggleFavouriteCommand { get; private set; }

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