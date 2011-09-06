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
            this.BackgroundImage = new Uri("PanoramaBackground.jpg", UriKind.Relative);
            this.Recent = new ObservableCollection<RecentMixViewModel>();
            this.Latest = new ObservableCollection<MixViewModel>();
            this.Tags = new ObservableCollection<TagViewModel>();
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

        public ObservableCollection<RecentMixViewModel> Recent { get; private set; }

        /// <summary>
        /// Gets the collection of new mixes.
        /// </summary>
        public ObservableCollection<MixViewModel> Latest { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<TagViewModel> Tags { get; private set; }

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
            this.LoadRecentMixes();
        }

        private void LoadRecentMixes()
        {
            var nowPlaying = PlayerService.Load();
            if (nowPlaying != null)
            {
                this.Recent.Add(
                    new RecentMixViewModel()
                        { 
                            IsNowPlaying = true, 
                            MixId = nowPlaying.MixId, 
                            MixName = nowPlaying.MixName,
                            TileTitle = nowPlaying.MixName, 
                            ImageUrl = nowPlaying.Cover.OriginalUrl,
                            ThumbnailUrl = nowPlaying.Cover.ThumbnailUrl,
                            NavigationUrl = new Uri("/PlayPage.xaml?mix=" + nowPlaying.MixId, UriKind.Relative)
                        });
            }

            var mixes = from response in Observable.Start(() => Json.Deserialize<MixesResponseContract>(Storage.Load("recentmixes.json")))
                        where response != null && response.Mixes != null
                        from mix in response.Mixes.ToObservable()
                        select new RecentMixViewModel(mix);
            mixes.Subscribe(this.Recent.Add, this.ShowError, this.LoadLatestMixes);
        }

        private void LoadLatestMixes()
        {
            var pageData = from latest in Downloader.GetJson<MixesResponseContract>(new Uri("http://8tracks.com/mixes.json", UriKind.RelativeOrAbsolute)).ObserveOnDispatcher().Do(_ =>
            {
                this.Latest.Clear();
            })
                           from mix in latest.Mixes.ToObservable(Scheduler.ThreadPool)
                           select new MixViewModel(mix);
            pageData.ObserveOnDispatcher().Subscribe(
                m => this.Latest.Add(m),
                this.ShowError,
                this.LoadTags);
        }

        /// <summary>
        /// </summary>
        private void LoadTags()
        {
            this.Tags.Clear();
            var tags = TagViewModel.SplitAndMergeIntoTags(this.Latest.Select(m => m.Tags)).OrderBy(t => t.TagName);

            foreach (var tag in tags)
            {
                this.Tags.Add(tag);
            }

            this.Tags.Add(new TagViewModel("more..."));

            this.HideProgress();
        }

        /// <summary>
        /// </summary>
        private void PickNewBackgroundImage()
        {
            var next = this.Latest.Skip(RandomNumber.Next(this.Latest.Count - 1)).FirstOrDefault();
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

    public class RecentMixViewModel : MixViewModel
    {
        /// <summary>
        /// Initializes a new instance of the RecentMixViewModel class.
        /// </summary>
        public RecentMixViewModel()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the RecentMixViewModel class.
        /// </summary>
        public RecentMixViewModel(MixContract mix) : base(mix)
        {
            

        }

        private bool isNowPlaying;

        public bool IsNowPlaying
        {
            get
            {
                return this.isNowPlaying;
            }
            set
            {
                if (this.isNowPlaying == value)
                {
                    return;
                }

                this.isNowPlaying = value;
                this.OnPropertyChanged("IsNowPlaying");
            }
        }
    }
}