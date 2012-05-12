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

    using FlatBeats.Framework;

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
            var progress = new[] 
            { 
                this.Hot.IsInProgressChanges,
                this.Recent.IsInProgressChanges,
                this.Popular.IsInProgressChanges
            };

            this.AddToLifetime(progress.Merge().Subscribe(_ => this.UpdateIsInProgress()));
            ////if (this.IsDataLoaded && this.Tag == loadTag && this.SearchQuery == loadQuery)
            ////{
            ////    return;
            ////}

            this.Tag = loadTag;
            this.SearchQuery = loadQuery;
            this.Title = (this.Tag ?? this.SearchQuery ?? string.Empty).ToUpper();

            if (!string.IsNullOrWhiteSpace(this.SearchQuery))
            {
                this.Recent.SearchQuery = this.SearchQuery;
                this.Hot.SearchQuery = this.SearchQuery;
                this.Popular.SearchQuery = this.SearchQuery;
            }
            else
            {
                this.Recent.Tag = this.Tag;
                this.Hot.Tag = this.Tag;
                this.Popular.Tag = this.Tag;
            }

            this.AddToLifetime(this.CurrentPanel.LoadAsync().Subscribe(_ => { }, this.HandleError, this.HideProgress));
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
            if (!mixList.IsLoaded)
            {
                this.AddToLifetime(mixList.LoadAsync().Subscribe(_ => { }, this.HandleError, this.HideProgress));
            }
        }
        #endregion

    }
}