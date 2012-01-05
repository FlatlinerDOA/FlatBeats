// --------------------------------------------------------------------------------------------------------------------
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

    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class MixesPageViewModel : PageViewModel
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
            this.Recent = new MixListViewModel()
            {
                Title = StringResources.Title_RecentMixes,
                Sort = "recent"
            };
            this.Hot = new MixListViewModel()
                {
                    Title = StringResources.Title_HotMixes,
                    Sort = "hot"
                };
            this.Popular = new MixListViewModel()
                {
                    Title = StringResources.Title_PopularMixes,
                    Sort = "popular"
                };
        }

        #endregion

        #region Public Properties

        public MixListViewModel Hot { get; private set; }

        public MixListViewModel Popular { get; private set; }

        public MixListViewModel Recent { get; private set; }

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

        private string searchQuery;

        public string SearchQuery
        {
            get
            {
                return this.searchQuery;
            }
            set
            {
                if (this.searchQuery == value)
                {
                    return;
                }

                this.searchQuery = value;
                this.OnPropertyChanged("SearchQuery");
            }
        }

        private int currentPanelIndex;

        public int CurrentPanelIndex
        {
            get
            {
                return this.currentPanelIndex;
            }
            set
            {
                if (this.currentPanelIndex == value)
                {
                    return;
                }

                this.currentPanelIndex = value;
                this.OnPropertyChanged("CurrentPanelIndex");
                this.LoadCurrentPanelSearchResults();
            }
        }
        #endregion

        #region Public Methods

        public override void Load()
        {
            this.Load(this.NavigationParameters["tag"], this.NavigationParameters["query"]);
        }

        public override void Unload()
        {
            base.Unload();
            this.AddToLifetime(null);
        }

        /// <summary>
        /// </summary>
        /// <param name="loadTag">
        /// </param>
        public void Load(string loadTag, string loadQuery)
        {
            if (this.IsDataLoaded && this.Tag == loadTag && this.SearchQuery == loadQuery)
            {
                return;
            }

            this.Tag = loadTag;
            this.SearchQuery = loadQuery;
            this.Title = (this.Tag ?? this.SearchQuery ?? string.Empty).ToUpper();

            this.SearchPanel(this.Recent);
            this.LoadCompleted();
        }


        private void LoadCurrentPanelSearchResults()
        {
            switch (this.currentPanelIndex)
            {
                case 0:
                    this.SearchPanel(this.Recent);
                    break;
                case 1:
                    this.SearchPanel(this.Hot);
                    break;
                case 2:
                    this.SearchPanel(this.Popular);
                    break;
            }
        }

        private void SearchPanel(MixListViewModel mixList)
        {
            if (mixList.IsDataLoaded)
            {
                return;
            }

            this.ShowProgress();
            if (!string.IsNullOrWhiteSpace(this.SearchQuery))
            {
                this.AddToLifetime(mixList.Search(this.SearchQuery).Subscribe(_ => { }, this.HandleError, this.HideProgress));
                mixList.LoadNextPage();
                return;
            }

            this.AddToLifetime(mixList.SearchByTag(this.Tag).Subscribe(_ => { }, this.HandleError, this.HideProgress));
            mixList.LoadNextPage();
        }
        #endregion

    }
}