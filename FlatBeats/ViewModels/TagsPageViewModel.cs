namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Reactive;

    public class TagsPageViewModel : PageViewModel
    {
        /// <summary>
        /// Initializes a new instance of the TagsPageViewModel class.
        /// </summary>
        public TagsPageViewModel()
        {
            this.Tags = new ObservableCollection<TagViewModel>();
            this.Title = "tags";
        }

        public void Load()
        {
            this.ShowProgress();
            var list = new List<TagViewModel>();
            var tags = from page in Observable.Range(1, 4)
                       from response in Downloader.GetJson<TagsResponseContract>(new Uri("http://8tracks.com/all/mixes/tags.json?sort=recent&tag_page=" + page))
                       from tag in response.Tags.ToObservable()
                       select new TagViewModel(tag.Name);
            tags.ObserveOnDispatcher().Subscribe(
                list.Add, 
                this.ShowError, 
                () =>
                {
                    foreach (var tag in list.OrderBy(tag => tag.TagName))
                    {
                        this.Tags.Add(tag);
                    }

                    this.HideProgress();
                });
        }


        public ObservableCollection<TagViewModel> Tags { get; set; }


    }
}
