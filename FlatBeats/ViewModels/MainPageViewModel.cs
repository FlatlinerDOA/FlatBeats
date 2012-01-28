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
        private int currentSectionIndex;

        private static readonly Uri DefaultBackground = new Uri("PanoramaBackground.jpg", UriKind.Relative);
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

        private Brush backgroundImage;

        private readonly Random random = new Random();

        private Uri backgroundUrl;

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
            this.backgroundUrl = this.State.GetValueOrDefault<Uri>("BackgroundUrl");

            this.AddToLifetime(this.Liked.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.Recent.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.Latest.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));

            if (this.IsDataLoaded)
            {
                this.Refresh();
                return;
            }

            this.LoadLikedPanel(); 
        }

        public override void Unload()
        {
            this.State["BackgroundUrl"] = this.backgroundUrl;
            this.Liked.Unload();
            this.Recent.Unload();
            this.Latest.Unload();
            this.TagsPanel.Unload();
            base.Unload();
        }

        private void LoadRecentAndLatestPanels()
        {
            this.ShowProgress(StringResources.Progress_Loading);
            var load = from recent in this.Recent.LoadAsync()
                       from latest in this.Latest.LoadAsync()
                       select latest;
            
            this.AddToLifetime(
                load.Delay(TimeSpan.FromSeconds(1)).ObserveOnDispatcher().Subscribe(
                    this.PickRandomBackground, 
                    this.HandleError,
                    this.LoadAllDataCompleted));
            //// ?? 
        }

        private void PickRandomBackground(IList<MixViewModel> results)
        {
            var url = this.Recent.Mixes.Where(p => p.IsNowPlaying).Select(p => p.ImageUrl).FirstOrDefault()
               ?? results.Where(mix => !mix.IsExplicit).Select(r => r.ImageUrl).Skip(this.random.Next(results.Count - 2)).FirstOrDefault()
               ?? DefaultBackground;
            if (url != this.backgroundUrl)
            {
                this.backgroundUrl = url;
                this.BackgroundImage = new ImageBrush
                    {
                        ImageSource = new BitmapImage(url) { CreateOptions = BitmapCreateOptions.DelayCreation },
                        Opacity = 0.4,
                        Stretch = Stretch.UniformToFill
                    };
            }
        }

        private void LoadAllDataCompleted()
        {
            if (this.Liked.Items.Any())
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

        private void LoadLikedPanel()
        {
            this.AddToLifetime(this.Liked.LoadAsync().Subscribe(_ => { }, this.HandleError, this.HideProgress));
            this.Liked.LoadFirstPage();
            this.LoadRecentAndLatestPanels();
        }

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

        private void Refresh()
        {
            this.LoadLikedPanel();
        }

        #endregion
    }
}