// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainPageViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class MainPageViewModel : ViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private static readonly Random RandomNumber = new Random();

        /// <summary>
        /// </summary>
        private Uri backgroundImage;

        /// <summary>
        /// </summary>
        private string message;

        /// <summary>
        /// </summary>
        private IDisposable subscription;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainPageViewModel()
        {
            this.BackgroundImage = new Uri("PanoramaBackground.jpg", UriKind.Relative);
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.MixRows = new ObservableCollection<DualTileRowViewModel<MixViewModel>>();
            this.Tags = new ObservableCollection<TagViewModel>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public Uri BackgroundImage
        {
            get
            {
                return this.backgroundImage;
            }

            set
            {
                if (this.backgroundImage == value)
                {
                    return;
                }

                this.backgroundImage = value;
                this.OnPropertyChanged("BackgroundImage");
            }
        }

        /// <summary>
        /// </summary>
        public bool IsDataLoaded { get; private set; }

        /// <summary>
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (this.message == value)
                {
                    return;
                }

                this.message = value;
                this.OnPropertyChanged("Message");
            }
        }

        private bool isInProgress;

        public bool IsInProgress
        {
            get
            {
                return this.isInProgress;
            }
            set
            {
                if (this.isInProgress == value)
                {
                    return;
                }

                this.isInProgress = value;
                this.OnPropertyChanged("IsInProgress");
            }
        }

        /// <summary>
        ///   Gets the collection of new mixes.
        /// </summary>
        public ObservableCollection<DualTileRowViewModel<MixViewModel>> MixRows { get; private set; }

        /// <summary>
        ///   Gets the collection of new mixes.
        /// </summary>
        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<TagViewModel> Tags { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public void Load()
        {
            this.LoadData();

            ////this.subscription = Observable.Interval(TimeSpan.FromSeconds(30))
            ////    .ObserveOnDispatcher()
            ////    .Subscribe(_ => this.PickNewBackgroundImage());
        }

        /// <summary>
        /// </summary>
        public void Unload()
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        private void LoadData()
        {
            if (this.IsDataLoaded)
            {
                return;
            }

            this.IsDataLoaded = true;
            this.IsInProgress = true;
            var pageData =
                from latest in
                    Downloader.DownloadJson<MixesContract>(
                        new Uri("http://8tracks.com/mixes.json", UriKind.RelativeOrAbsolute), "latestmixes.json")
                from mix in latest.Mixes.ToObservable()
                select new MixViewModel(mix);
            this.Mixes.Clear();
            pageData.ObserveOnDispatcher().Subscribe(
                m => this.Mixes.Add(m), ex => this.ShowError(ex.Message), this.LoadTags);
        }

        /// <summary>
        /// </summary>
        private void LoadTags()
        {
            ////this.MixRows.Clear();
            ////foreach (var row in DualTileRowViewModel<MixViewModel>.Tile(this.Mixes))
            ////{
            ////    this.MixRows.Add(row);
            ////}
            this.Tags.Clear();
            var tags = TagViewModel.SplitAndMergeIntoTags(this.Mixes.Select(m => m.Tags));

            foreach (var tag in tags)
            {
                this.Tags.Add(tag);
            }

            this.IsInProgress = false;
        }

        /// <summary>
        /// </summary>
        private void PickNewBackgroundImage()
        {
            var next = this.Mixes.Skip(RandomNumber.Next(this.Mixes.Count - 1)).FirstOrDefault();
            if (next == null)
            {
                return;
            }

            this.BackgroundImage = next.ImageUrl;
        }

        /// <summary>
        /// </summary>
        /// <param name="message">
        /// </param>
        private void ShowError(string message)
        {
            this.Message = message;
        }

        #endregion
    }
}