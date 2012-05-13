namespace FlatBeats.Users.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;
    using FlatBeats.ViewModels;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;

    public class UserProfileTracksViewModel: InfiniteScrollPanelViewModel<TrackViewModel, TrackContract>, ILifetime<string>
    {
        private readonly ProfileService profileService;

        private bool censor;

        /// <summary>
        /// Initializes a new instance of the UserProfileTracksViewModel class.
        /// </summary>
        public UserProfileTracksViewModel(bool isCurrentUser, ProfileService profileService)
        {
            this.profileService = profileService;
            this.IsCurrentUser = isCurrentUser;
            this.Title = StringResources.Title_FavoriteTracks;
        }

        public IObservable<Unit> LoadAsync(string userId)
        {
            this.UserId = userId;
            this.ShowProgress(StringResources.Progress_Loading);
            return from _ in this.profileService.GetSettingsAsync().Do(s => this.censor = s.CensorshipEnabled)
                   from load in this.LoadAsync()
                   select load;
        }

        protected bool IsCurrentUser { get; set; }

        protected string UserId { get; set; }

        protected override IObservable<IList<TrackContract>> GetPageOfItemsAsync(int pageNumber, int pageSize)
        {
            if (this.UserId == null)
            {
                return Observable.Empty<IList<TrackContract>>();
            }

            return this.profileService.GetFavouriteTracksAsync(this.UserId, pageNumber, pageSize).Select(p => (IList<TrackContract>)p.Tracks);
        }

        protected override void LoadItem(TrackViewModel viewModel, TrackContract data)
        {
            viewModel.Load(data, this.censor);
        }

        protected override void LoadPageCompleted()
        {
            if (this.Items.Count == 0)
            {
                if (this.IsCurrentUser)
                {
                    this.Message = StringResources.Message_YouHaveNoFavoriteTracks;
                }
                else
                {
                    this.Message = StringResources.Message_UserHasNoFavoriteTracks;
                }

                this.ShowMessage = true;
            }
            else
            {
                this.Message = null;
            }
        }
    }

    public class TrackViewModel : ListItemViewModel
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
            this.profileService.SetTrackFavouriteAsync(this.Id, this.IsFavourite).ObserveOnDispatcher().Subscribe(
                _ => { },
                ex => this.IsFavourite = !this.IsFavourite);
        }

        private void UpdateFavouriteState()
        {
            // TODO: Icon
            ////if (this.IsFavourite)
            ////{
            ////    this.ToggleFavouriteCommand.IconUrl = RemoveFromFavouritesIcon;
            ////}
            ////else
            ////{
            ////    this.ToggleFavouriteCommand.IconUrl = AddToFavouritesIcon;
            ////}

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
