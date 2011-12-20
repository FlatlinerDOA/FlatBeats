namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;

    using FlatBeats.DataModel;

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
            var tagViewModels = from pageNumber in Observable.Range(1, 5)
                                from response in Downloader.GetJson<TagsResponseContract>(new Uri("http://8tracks.com/all/mixes/tags.json?sort=recent&tag_page=" + pageNumber))
                                from tag in response.Tags.ToObservable()
                                select new TagViewModel(tag.Name);


            var list = new List<TagViewModel>();
            this.subscription = tagViewModels.ObserveOnDispatcher().Subscribe(
                list.Add, this.ShowError, () => 
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