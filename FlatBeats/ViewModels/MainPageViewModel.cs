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

    using FlatBeats.Controls;

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
            this.TagsPanel = new MainPageTagsViewModel();
            this.BackgroundImage = new Uri("PanoramaBackground.jpg", UriKind.Relative);
            this.Title = "flat beats";
            ////this.ApplicationBarButtonCommands = new ObservableCollection<ICommandLink>();
            ////this.ApplicationBarMenuCommands = new ObservableCollection<ICommandLink>
            ////    {
            ////        new CommandLink()
            ////            {
            ////                Command = new DelegateCommand(this.ShowSettings), 
            ////                Text = "profile"
            ////            }
            ////    };
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
        /// </summary>
        public bool IsDataLoaded { get; private set; }

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
        /// </summary>
        public void Load()
        {
            this.LoadData();
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
                this.ShowProgress();

                var reload = from liked in this.Liked.LoadAsync()
                             from recent in this.Recent.LoadAsync()
                             select new Unit();
                reload.ObserveOnDispatcher().Subscribe(_ => { }, this.ShowError, this.HideProgress);
                return;
            }

            this.IsDataLoaded = true;
            this.ShowProgress();
            var load = from liked in this.Liked.LoadAsync()
                       from recent in this.Recent.LoadAsync()
                       from latest in this.Latest.LoadAsync()
                       from tags in Observable.Start(() => this.TagsPanel.Load(latest))
                       select new Unit();
            load.ObserveOnDispatcher().Subscribe(_ => { }, this.ShowError, this.HideProgress);
        }

        #endregion

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

        public ObservableCollection<ICommandLink> ApplicationBarButtonCommands { get; private set; }

        public ObservableCollection<ICommandLink> ApplicationBarMenuCommands { get; private set; }
    }
}