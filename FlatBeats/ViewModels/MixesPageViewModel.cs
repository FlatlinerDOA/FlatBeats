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
            string queryString, tagQueryString;
            this.NavigationParameters.TryGetValue("tag", out tagQueryString);
            this.NavigationParameters.TryGetValue("q", out queryString);

            this.Load(tagQueryString, queryString);
        }

        private void UpdateIsInProgress()
        {
            if (this.CurrentPanel.IsInProgress)
            {
                this.ShowProgress(this.CurrentPanel.InProgressMessage);
            }
            else
            {
                this.HideProgress();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="loadTag">
        /// </param>
        public void Load(string loadTag, string loadQuery)
        {
            this.AddToLifetime(this.Hot.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.Recent.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));
            this.AddToLifetime(this.Popular.IsInProgressChanges.Subscribe(_ => this.UpdateIsInProgress()));

            if (this.IsDataLoaded && this.Tag == loadTag && this.SearchQuery == loadQuery)
            {
                return;
            }

            this.LoadCompleted();

            this.Tag = loadTag;
            this.SearchQuery = loadQuery;
            this.Title = (this.Tag ?? this.SearchQuery ?? string.Empty).ToUpper();

            this.SearchPanel(this.Recent);
        }

        public MixListViewModel CurrentPanel 
        {
            get
        {
            switch (this.currentPanelIndex)
            {
                case 0:
                    return this.Recent;
                case 1:
                    return this.Hot;
                case 2:
                    return this.Popular;
            }

            return null;
        }
        }

        private void LoadCurrentPanelSearchResults()
        {
            this.SearchPanel(this.CurrentPanel);
        }

        private void SearchPanel(MixListViewModel mixList)
        {
            if (mixList.IsDataLoaded)
            {
                return;
            }

            this.ShowProgress(mixList.InProgressMessage);
            if (!string.IsNullOrWhiteSpace(this.SearchQuery))
            {
                mixList.SearchQuery = this.SearchQuery;
            }
            else
            {
                mixList.Tag = this.Tag;
            }

            this.AddToLifetime(mixList.LoadAsync().Subscribe(_ => { }, this.HandleError, this.HideProgress));
            mixList.LoadFirstPage();
        }
        #endregion

    }
}