﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MixesPageViewModel.cs" company="">
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
    using System.Net;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class MixesPageViewModel : ViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private string tag;

        /// <summary>
        /// </summary>
        private string title;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the MixesPageViewModel class.
        /// </summary>
        public MixesPageViewModel()
        {
            this.HotMixes = new ObservableCollection<MixViewModel>();
            this.RecentMixes = new ObservableCollection<MixViewModel>();
            this.PopularMixes = new ObservableCollection<MixViewModel>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public ObservableCollection<MixViewModel> HotMixes { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsDataLoaded { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<MixViewModel> PopularMixes { get; private set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<MixViewModel> RecentMixes { get; private set; }

        /// <summary>
        /// </summary>
        public string Tag
        {
            get
            {
                return this.tag;
            }

            set
            {
                if (this.tag == value)
                {
                    return;
                }

                this.tag = value;
                this.OnPropertyChanged("Tag");
            }
        }

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

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="loadTag">
        /// </param>
        public void Load(string loadTag)
        {
            if (this.IsDataLoaded && this.Tag == loadTag)
            {
                return;
            }

            this.IsDataLoaded = true;
            this.Tag = loadTag;
            this.LoadData();
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            this.IsInProgress = true;
            this.Title = this.Tag.ToUpper();

            DownloadMixes(this.Tag, "recent").ObserveOnDispatcher().Subscribe(
                this.LoadRecentMixes, 
                () => DownloadMixes(this.Tag, "hot").ObserveOnDispatcher().Subscribe(
                    this.LoadHotMixes, 
                    () => DownloadMixes(this.Tag, "popular").ObserveOnDispatcher().Subscribe(
                        this.LoadPopularMixes, 
                        () =>
                            {
                                this.IsInProgress = false;
                            })));
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="tag">
        /// </param>
        /// <param name="sort">
        /// </param>
        /// <returns>
        /// </returns>
        private static IObservable<MixesResponseContract> DownloadMixes(string tag, string sort)
        {
            return Downloader.GetJson<MixesResponseContract>(
                new Uri(
                    string.Format("http://8tracks.com/mixes.json?tag={0}&sort={1}", tag, sort),
                    UriKind.RelativeOrAbsolute), sort + "mixes.json");
        }

        /// <summary>
        /// </summary>
        /// <param name="mixes">
        /// </param>
        private void LoadHotMixes(MixesResponseContract mixes)
        {
            foreach (var mix in mixes.Mixes.Select(m => new MixViewModel(m)))
            {
                this.HotMixes.Add(mix);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="mixes">
        /// </param>
        private void LoadPopularMixes(MixesResponseContract mixes)
        {
            foreach (var mix in mixes.Mixes.Select(m => new MixViewModel(m)))
            {
                this.PopularMixes.Add(mix);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="mixes">
        /// </param>
        private void LoadRecentMixes(MixesResponseContract mixes)
        {
            foreach (var mix in mixes.Mixes.Select(m => new MixViewModel(m)))
            {
                this.RecentMixes.Add(mix);
            }
        }

        #endregion
    }
}