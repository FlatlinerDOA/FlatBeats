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
        private int currentSectionIndex;

        /// <summary>
        /// </summary>
        private IDisposable subscription = Disposable.Empty;

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
            this.TagsPanel = new MainPageTagsViewModel();
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

        /// <summary>
        ///   Gets the latest mixes panel.
        /// </summary>
        public MainPageLatestViewModel Latest { get; private set; }

        /// <summary>
        ///   Gets the liked mixes panel
        /// </summary>
        public MainPageLikedViewModel Liked { get; private set; }

        /// <summary>
        ///   Gets the recent mixes panel
        /// </summary>
        public MainPageRecentViewModel Recent { get; private set; }

        /// <summary>
        ///   Gets the tags panel
        /// </summary>
        public MainPageTagsViewModel TagsPanel { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public override void Load()
        {
            this.subscription.Dispose();
            if (this.IsDataLoaded)
            {
                this.Refresh();
                return;
            }

            this.ShowProgress();
            var load = from liked in this.Liked.LoadAsync()
                       from recent in this.Recent.LoadAsync()
                       from latest in this.Latest.LoadAsync()
                       select latest;
            this.subscription = load.ObserveOnDispatcher().Subscribe(
                latest => this.TagsPanel.Load(latest), this.ShowError, this.LoadCompleted);
        }

        private void Refresh()
        {
            this.ShowProgress();

            var reload = from liked in this.Liked.LoadAsync()
                         from recent in this.Recent.LoadAsync()
                         select new Unit();
            this.subscription = reload.ObserveOnDispatcher().Subscribe(_ => { }, this.ShowError, this.LoadCompleted);
        }

        /// <summary>
        /// </summary>
        public override void Unload()
        {
            this.subscription.Dispose();
        }

        #endregion
    }
}