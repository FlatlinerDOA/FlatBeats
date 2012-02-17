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
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using FlatBeats.Framework;

    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class MainPageViewModel : PageViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private static readonly Uri DefaultBackground = new Uri("PanoramaBackground.jpg", UriKind.Relative);

        /// <summary>
        /// </summary>
        private readonly Random random = new Random();

        /// <summary>
        /// </summary>
        private Brush backgroundImage;

        /// <summary>
        /// </summary>
        private Uri backgroundImageUrl;

        public Uri BackgroundImageUrl
        {
            get
            {
                return this.backgroundImageUrl;
            }
            set
            {
                if (this.backgroundImageUrl == value)
                {
                    return;
                }

                this.backgroundImageUrl = value;
                this.OnPropertyChanged("BackgroundImageUrl");
            }
        }
        /// <summary>
        /// </summary>
        private int currentSectionIndex;

        private bool latestDisplayed;

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
            this.Title = "flat beats";
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public Brush BackgroundImage
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
                if (this.CurrentSectionIndex == 1 || this.CurrentSectionIndex == 3)
                {
                    this.EnsureLatestDisplayed();
                }
            }
        }

        private void EnsureLatestDisplayed()
        {
            if (this.latestDisplayed)
            {
                return;
            }

            this.latestDisplayed = true;
            this.Latest.Display();
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
            this.BackgroundImageUrl = this.State.GetValueOrDefault<Uri>("BackgroundUrl");

            this.AddToLifetime(this.Liked.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.Recent.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.Latest.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));

            this.AddToLifetime(this.Liked.LoadAsync().Subscribe(_ => { }, this.HandleError, this.HideProgress));
            if (this.IsDataLoaded)
            {
                this.Liked.LoadFirstPage();
                this.AddToLifetime(this.Recent.LoadAsync().Subscribe(_ => this.PickRandomBackground(), this.HandleError, this.HideProgress));
                return;
            }

            var pageLoad = from first in
                    Observable.Timer(TimeSpan.FromMilliseconds(250)).ObserveOnDispatcher().Do(
                        _ => this.Liked.LoadFirstPage())
                from second in
                    Observable.Timer(TimeSpan.FromMilliseconds(500)).ObserveOnDispatcher().Do(
                        _ => this.LoadRecentAndLatestPanels())
                select new Unit();

            this.AddToLifetime(pageLoad.Subscribe(_ => { }, this.HandleError));


        }

        /// <summary>
        /// </summary>
        public override void Unload()
        {
            if (this.State != null)
            {
                this.State["BackgroundUrl"] = this.BackgroundImageUrl;
            }

            this.Liked.Unload();
            this.Recent.Unload();
            this.Latest.Unload();
            this.TagsPanel.Unload();
            base.Unload();
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        private void LoadAllDataCompleted()
        {
            if (this.Liked.Items.Count != 0)
            {
                this.TagsPanel.Title = StringResources.Title_LikedTags;
                this.TagsPanel.Load(this.Liked.Items);
            }
            else
            {
                this.TagsPanel.Title = StringResources.Title_LatestTags;
                this.TagsPanel.Load(this.Latest.Mixes);
            }

            this.LoadCompleted();
        }

        /// <summary>
        /// </summary>
        private void LoadRecentAndLatestPanels()
        {
            this.ShowProgress(StringResources.Progress_Loading);
            var load = from recent in this.Recent.LoadAsync() 
                       from latest in this.Latest.LoadAsync() 
                       select latest;

            this.AddToLifetime(
                load.ObserveOnDispatcher().Subscribe(_ =>
                    this.PickRandomBackground(), this.HandleError, this.LoadAllDataCompleted));
        }

        /// <summary>
        /// </summary>
        /// <param name="results">
        /// </param>
        private void PickRandomBackground()
        {
            var url = this.Recent.Mixes.Where(p => p.IsNowPlaying).Select(p => p.ImageUrl).FirstOrDefault() ?? 
                this.BackgroundImageUrl ?? 
                this.Latest.Mixes.Where(mix => !mix.IsExplicit).Select(r => r.ImageUrl).Skip(this.random.Next(this.Latest.Mixes.Count - 2)).FirstOrDefault() ?? 
                DefaultBackground;
            if (this.BackgroundImageUrl != url || this.BackgroundImage == null)
            {
                this.BackgroundImageUrl = url;
                this.BackgroundImage = new ImageBrush
                    {
                        ImageSource = new BitmapImage(url) { CreateOptions = BitmapCreateOptions.DelayCreation },
                        Opacity = 0.4,
                        Stretch = Stretch.UniformToFill
                    };
            }
        }

        /// <summary>
        /// </summary>
        private void UpdateIsInProgress()
        {
            if (this.Liked.IsInProgress)
            {
                this.ShowProgress(StringResources.Progress_Loading);
                return;
            }

            if (this.Recent.IsInProgress)
            {
                this.ShowProgress(StringResources.Progress_Loading);
                return;
            }

            if (this.Latest.IsInProgress)
            {
                this.ShowProgress(StringResources.Progress_Loading);
                return;
            }

            this.HideProgress();
        }

        #endregion
    }
}