namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Text.RegularExpressions;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.ViewModels;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;

    public class TrackViewModel : ListItemViewModel, INavigationItem
    {
        private readonly IAsyncDownloader downloader;

        private readonly ProfileService profileService;

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

        public TrackViewModel()
            : this(AsyncDownloader.Instance, ProfileService.Instance)
        {
        }

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the TrackViewModel class.
        /// </summary>
        public TrackViewModel(IAsyncDownloader downloader, ProfileService profileService)
        {
            this.downloader = downloader;
            this.profileService = profileService;
            this.ToggleFavouriteCommand = new CommandLink()
                {
                    Command = new DelegateCommand(
                        this.ToggleFavourite,
                        () => this.downloader.IsAuthenticated),
                    IconUrl = AddToFavouritesIcon,
                    HideWhenInactive = !IsInDesignMode
                };
        }

        /// <summary>
        /// Initializes a new instance of the TrackViewModel class.
        /// </summary>
        /// <param name="track">
        /// </param>
        public TrackViewModel(TrackContract track, bool censor)
            : this()
        {
            this.Load(track, censor);
        }

        public void Load(TrackContract track, bool censor)
        {
            this.title = (track.Name ?? string.Empty).Trim();
            this.artist = (track.Artist ?? string.Empty).Trim();
            this.Id = track.Id;
            const string Pattern = @"\((.|\n)*?\)";

            var simplifiedArtistName = track.Artist != null ? Regex.Replace(track.Artist, Pattern, string.Empty) : string.Empty;
            var simplifiedTrackName = track.Name != null ? Regex.Replace(track.Name, Pattern, string.Empty) : string.Empty;
            this.NavigationUrl = new Uri("music://zune?" + simplifiedArtistName + " " + simplifiedTrackName, UriKind.Absolute);
            this.IsFavourite = track.IsFavourite;
        }


        private void ToggleFavourite()
        {
            this.IsFavourite = !this.IsFavourite;
            this.profileService.SetTrackFavouriteAsync(this.Id, this.IsFavourite).ObserveOnDispatcher().Subscribe(
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
            get;
            set;
        }
    }
}