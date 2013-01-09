namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;
    using FlatBeats.Framework;

    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public sealed class TagsPageViewModel : PageViewModel
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
            if (this.IsDataLoaded)
            {
                return;
            }

            this.ShowProgress(StringResources.Progress_Loading);
            var tagViewModels = from pageNumber in Observable.Range(1, 5, Scheduler.Immediate)
                                from response in MixesService.GetTagsAsync(pageNumber)
                                where response != null && response.Tags != null
                                from tag in response.Tags.ToObservable()
                                select new TagViewModel(tag.Name);


            var list = new List<TagViewModel>();
            this.subscription = tagViewModels.ObserveOnDispatcher().Subscribe(
                list.Add, this.HandleError, () => 
                { 
                    var t = new TagsByFirstLetter(list);
                    this.Tags = t;

                    this.LoadCompleted();

                    if (list.Count == 0)
                    {
                        this.Message = StringResources.Message_NoTagsFound;
                        this.ShowMessage = true;
                    }
                    else
                    {
                        this.Message = null;
                    }
                });
        }

        public override void Unload()
        {
            this.subscription.Dispose();
        }

        #endregion
    }
}