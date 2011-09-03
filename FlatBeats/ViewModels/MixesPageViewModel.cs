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
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.Phone.Reactive;

    using FlatBeats.DataModel;

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
            this.Hot = new MixListViewModel() { Sort = "hot" };
            this.Recent = new MixListViewModel() { Sort = "recent" };
            this.Popular = new MixListViewModel() { Sort = "popular" };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public bool IsDataLoaded { get; private set; }

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
                switch (this.currentPanelIndex)
                {
                    case 0:
                        this.ShowProgress();
                        this.Recent.SearchByTag(this.Tag).Subscribe(_ => { }, this.HideProgress);
                        break;
                    case 1:
                        this.ShowProgress();
                        this.Hot.SearchByTag(this.Tag).Subscribe(_ => { }, this.HideProgress);
                        break;
                    case 2:
                        this.ShowProgress();
                        this.Popular.SearchByTag(this.Tag).Subscribe(_ => { }, this.HideProgress);
                        break;
                }
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
            this.Title = this.Tag.ToUpper();

            this.ShowProgress();
            this.Recent.SearchByTag(this.Tag).Subscribe(_ => { }, this.HideProgress);
        }

        #endregion

    }
}