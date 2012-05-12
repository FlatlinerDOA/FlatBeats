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
        private readonly IAsyncDownloader downloader;

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
        public TagsPageViewModel() : this(Downloader.Instance)
        {

        }

        /// <summary>
        ///   Initializes a new instance of the TagsPageViewModel class.
        /// </summary>
        public TagsPageViewModel(IAsyncDownloader downloader)
        {
            this.downloader = downloader;
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
            this.ShowProgress(StringResources.Progress_Loading);
            var tagViewModels = from pageNumber in Observable.Range(1, 5)
                                from response in MixesService.GetTagsAsync(pageNumber)
                                from tag in response.Tags.ToObservable()
                                select new TagViewModel(tag.Name);


            var list = new List<TagViewModel>();
            this.subscription = tagViewModels.ObserveOnDispatcher().Subscribe(
                list.Add, this.HandleError, () => 
                { 
                    var t = new TagsByFirstLetter(list);
                    this.Tags = t;
                    this.LoadCompleted();
                });
        }

        public override void Unload()
        {
            this.subscription.Dispose();
        }

        #endregion
    }
}