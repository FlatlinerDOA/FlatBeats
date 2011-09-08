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

    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class MainPageViewModel : PageViewModel
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
            this.Liked = new MainPageLikedViewModel();
            this.Recent = new MainPageRecentViewModel();
            this.Latest = new MainPageLatestViewModel();
            this.Tags = new MainPageTagsViewModel();
            this.BackgroundImage = new Uri("PanoramaBackground.jpg", UriKind.Relative);
            this.Title = "flat beats";
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
        /// Gets the liked mixes panel
        /// </summary>
        public MainPageLikedViewModel Liked { get; private set; }

        /// <summary>
        /// Gets the recent mixes panel
        /// </summary>
        public MainPageRecentViewModel Recent { get; private set; }

        /// <summary>
        /// Gets the latest mixes panel.
        /// </summary>
        public MainPageLatestViewModel Latest { get; private set; }

        /// <summary>
        /// Gets the tags panel
        /// </summary>
        public MainPageTagsViewModel Tags { get; private set; }

        private int currentSectionIndex;

        public int CurrentSectionIndex
        {
            get
            {
                return this.currentSectionIndex;
            }
            set
            {
                if (this.currentSectionIndex == value)
                {
                    return;
                }

                this.currentSectionIndex = value;
                this.OnPropertyChanged("CurrentSectionIndex");
            }
        }
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
            this.ShowProgress();
            var load = from recent in Observable.Start(this.Recent.Load)
                       from likedDelay in Observable.Timer(TimeSpan.FromSeconds(1))
                       from liked in Observable.Start(this.Liked.Load)
                       from latestDelay in Observable.Timer(TimeSpan.FromSeconds(1))
                       from latest in Observable.Start(this.Latest.Load)
                       from latestLoaded in this.Latest.Loaded
                       from tags in Observable.Start(() => this.Tags.Load(this.Latest.Mixes))
                       select new Unit();
            load.Subscribe();
        }


        /////// <summary>
        /////// </summary>
        ////private void PickNewBackgroundImage()
        ////{
        ////    var next = this.Latest.Skip(RandomNumber.Next(this.Latest.Count - 1)).FirstOrDefault();
        ////    if (next == null)
        ////    {
        ////        return;
        ////    }

        ////    this.BackgroundImage = next.ImageUrl;
        ////}

        #endregion
    }
}