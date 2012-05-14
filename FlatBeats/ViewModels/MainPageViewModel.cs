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
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Flatliner.Phone;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public sealed class MainPageViewModel : PageViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private static readonly Uri DefaultBackground = new Uri("PanoramaBackground.jpg", UriKind.Relative);

        /// <summary>
        /// </summary>
        private readonly ProfileService profileService;

        /// <summary>
        /// </summary>
        private readonly Random random = new Random();

        /// <summary>
        /// </summary>
        private static volatile bool HasCheckedForRating;

        /// <summary>
        /// </summary>
        private Brush backgroundImage;

        /// <summary>
        /// </summary>
        private Uri backgroundImageUrl;

        /// <summary>
        /// </summary>
        private int currentSectionIndex;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public MainPageViewModel()
            : this(ProfileService.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// <param name="profileService">
        /// </param>
        public MainPageViewModel(ProfileService profileService)
        {
            this.profileService = profileService;
            this.Liked = new MainPageLikedViewModel { Opacity = 1 };
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

                this.Liked.Opacity = this.InRangeOf(0);
                this.Recent.Opacity = this.InRangeOf(1);
                this.Latest.Opacity = this.InRangeOf(2);
                this.TagsPanel.Opacity = this.InRangeOf(3);
            }
        }

        /// <summary>
        /// Gets the latest mixes panel.
        /// </summary>
        public MainPageLatestViewModel Latest { get; private set; }

        /// <summary>
        /// Gets the liked mixes panel
        /// </summary>
        public MainPageLikedViewModel Liked { get; private set; }

        /// <summary>
        /// Gets the recent mixes panel
        /// </summary>
        public MainPageRecentViewModel Recent { get; private set; }

        /// <summary>
        /// Gets the tags panel
        /// </summary>
        public MainPageTagsViewModel TagsPanel { get; private set; }

        /// <summary>
        /// Gets the current user id
        /// </summary>
        public string UserId { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public override void Load()
        {
            this.BackgroundImageUrl = this.State.GetValueOrDefault<Uri>("BackgroundUrl");

            var progressSources = new[]
                {
                   this.Liked.IsInProgressChanges, this.Recent.IsInProgressChanges, this.Latest.IsInProgressChanges 
                };

            this.AddToLifetime(progressSources.Merge().Subscribe(_ => this.UpdateIsInProgress()));

            if (this.IsDataLoaded)
            {
                IObservable<IObservable<Unit>> likedLoad =
                    from userToken in
                        this.profileService.LoadUserTokenAsync().DefaultIfEmpty().ObserveOnDispatcher().Do(
                            u => this.UserId = u != null ? u.CurrentUser.Id : null)
                    from liked in this.Liked.LoadAsync(this.UserId)
                    select ObservableEx.SingleUnit();
                this.AddToLifetime(likedLoad.Subscribe(_ => { }, this.HandleError, this.HideProgress));
                this.AddToLifetime(
                    this.Recent.LoadAsync().Subscribe(
                        _ => this.PickRandomBackground(), this.HandleError, this.HideProgress));
                return;
            }

            this.CheckForRating();

            IObservable<IObservable<Unit>> pageLoad =
                from first in Observable.Timer(TimeSpan.FromMilliseconds(500)).ObserveOnDispatcher()
                from userToken in
                    this.profileService.LoadUserTokenAsync().DefaultIfEmpty().ObserveOnDispatcher().Do(
                        u => this.UserId = u != null && u.CurrentUser != null ? u.CurrentUser.Id : null)
                from liked in this.Liked.LoadAsync(this.UserId)
                select ObservableEx.SingleUnit();

            this.AddToLifetime(pageLoad.Subscribe(_ => { }, this.HandleError, this.LoadCompleted));
            IObservable<IObservable<Unit>> delayedLoad =
                from second in
                    Observable.Timer(TimeSpan.FromSeconds(3)).ObserveOnDispatcher().Do(
                        _ => this.LoadRecentAndLatestPanels())
                select ObservableEx.SingleUnit();
            this.AddToLifetime(delayedLoad.Subscribe(_ => { }, this.HandleError, this.LoadCompleted));
        }

        /// <summary>
        /// </summary>
        public override void Unload()
        {
            if (this.State != null)
            {
                this.State["BackgroundUrl"] = this.BackgroundImageUrl;
            }

            this.BackgroundImage = null;
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
        private void CheckForRating()
        {
            if (HasCheckedForRating)
            {
                return;
            }

            HasCheckedForRating = true;

            var rateApp = this.profileService.GetSettingsAsync().ObserveOnDispatcher().SelectMany(
                s =>
                    {
                        if (!s.PromptedToRate)
                        {
                            s.StartCount++;
                            if (s.StartCount >= 10)
                            {
                                s.PromptedToRate = true;
                                var response =
                                    MessageBox.Show(
                                        StringResources.MessageBox_RateThisApp_Message,
                                        StringResources.MessageBox_RateThisApp_Title,
                                        MessageBoxButton.OKCancel);

                                return this.profileService.SaveSettingsAsync(s).ObserveOnDispatcher().Do(
                                    _ =>
                                        {
                                            if (response == MessageBoxResult.OK)
                                            {
                                                this.Navigation.NavigateTo(
                                                    new Uri("rate:application", UriKind.Absolute));
                                            }
                                        });
                            }

                            return this.profileService.SaveSettingsAsync(s).ObserveOnDispatcher();
                        }

                        return Observable.Empty<Unit>();
                    });

            this.AddToLifetime(rateApp.Subscribe(_ => { }, this.HandleError));
        }

        /// <summary>
        /// </summary>
        /// <param name="section">
        /// </param>
        /// <returns>
        /// </returns>
        private int InRangeOf(int section)
        {
            return ((Math.Abs(this.CurrentSectionIndex - section) + 3) % 3) > 1 ? 0 : 1;
        }

        /// <summary>
        /// </summary>
        private void LoadAllDataCompleted()
        {
            if (this.Liked.Items.Count >= 5)
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
            IObservable<IList<MixViewModel>> load = from recent in this.Recent.LoadAsync()
                                                    from latest in this.Latest.LoadAsync()
                                                    select latest;

            this.AddToLifetime(
                load.ObserveOnDispatcher().Subscribe(
                    _ => this.PickRandomBackground(), this.HandleError, this.LoadAllDataCompleted));
        }

        /// <summary>
        /// </summary>
        private void PickRandomBackground()
        {
            Uri url = this.Recent.Mixes.Where(p => p.IsNowPlaying).Select(p => p.ImageUrl).FirstOrDefault()
                      ??
                      this.BackgroundImageUrl
                      ??
                      this.Latest.Mixes.Where(mix => !mix.IsExplicit).Select(r => r.ImageUrl).Skip(
                          this.random.Next(this.Latest.Mixes.Count - 2)).FirstOrDefault() ?? DefaultBackground;
            if (this.BackgroundImageUrl != url || this.BackgroundImage == null)
            {
                this.BackgroundImageUrl = url;
                this.BackgroundImage = new ImageBrush
                    {
                        ImageSource = new BitmapImage(url) { CreateOptions = BitmapCreateOptions.DelayCreation }, 
                        Opacity = 0.3, 
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