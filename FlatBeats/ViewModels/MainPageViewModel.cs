// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EightTracks.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;

    using EightTracks.DataModel;

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

        private IDisposable subscription;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainPageViewModel()
        {
            this.BackgroundImage = new Uri("PanoramaBackground.png", UriKind.Relative);
            this.Mixes = new ObservableCollection<MixViewModel>();
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
        ///   Gets the collection of new mixes.
        /// </summary>
        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<TagViewModel> Tags { get; private set; }

        #endregion

        #region Public Methods

        public void Load()
        {
            this.LoadData();
            this.subscription = Observable.Interval(TimeSpan.FromSeconds(30))
                .ObserveOnDispatcher()
                .Subscribe(_ => this.PickNewBackgroundImage());
        }

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

            Downloader.DownloadJson<MixesContract>(new Uri("http://8tracks.com/mixes.json", UriKind.RelativeOrAbsolute), "latestmixes.json")
                .ObserveOnDispatcher()
                .Subscribe(this.LoadMixes); 
        }

        public void Unload()
        {
            this.subscription.Dispose();
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="mixes">
        /// </param>
        private void LoadMixes(MixesContract mixes)
        {
            this.Mixes.Clear();
            foreach (var mix in mixes.Mixes.Select(m => new MixViewModel(m)))
            {
                this.Mixes.Add(mix);
            }

            this.Tags.Clear();
            var tags = TagViewModel.SplitAndMergeIntoTags(
                mixes.Mixes.Select(m => m.Tags));
            
            foreach (var tag in tags)
            {
                this.Tags.Add(tag);
            }
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

        #endregion
    }
}