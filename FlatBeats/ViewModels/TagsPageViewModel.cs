// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TagsPageViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.ViewModels
{
    using System;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class TagsPageViewModel : PageViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private TagsByFirstLetter tags;

        private IDisposable subscription = Disposable.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the TagsPageViewModel class.
        /// </summary>
        public TagsPageViewModel()
        {
            ////this.Tags = new ObservableCollection<Grouping<string, TagViewModel>>();
            this.Title = "tags";
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public TagsByFirstLetter Tags
        {
            get
            {
                return this.tags;
            }

            set
            {
                if (this.tags == value)
                {
                    return;
                }

                this.tags = value;
                this.OnPropertyChanged("Tags");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public override void Load()
        {
            this.ShowProgress();
            this.subscription = Observable.Start(() => new TagsByFirstLetter()).ObserveOnDispatcher().Subscribe(
                t =>
                    {
                        this.Tags = t;
                    }, 
                    this.ShowError, 
                    this.LoadCompleted);
        }

        public override void Unload()
        {
            this.subscription.Dispose();
        }

        #endregion
    }
}