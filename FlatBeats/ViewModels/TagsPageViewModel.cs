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
        private static readonly string Groups = "#abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Initializes a new instance of the TagsPageViewModel class.
        /// </summary>
        public TagsPageViewModel()
        {
            this.Tags = new ObservableCollection<Grouping<string, TagViewModel>>();
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
                        list.Sort((a, b) => string.Compare(a.TagName, b.TagName, StringComparison.OrdinalIgnoreCase));
                        var sorted = from c in Groups
                                     join tag in list on c.ToString() equals tag.Key
                                     into jt
                                     from t in jt 
                                     group t by c.ToString() into g
                                     orderby g.Key
                                    select new Grouping<string, TagViewModel>(g);
                    foreach (var tag in sorted)
                    {
                        this.Tags.Add(tag);
                    }

                    this.HideProgress();
                });
        }


        public ObservableCollection<Grouping<string, TagViewModel>> Tags { get; set; }


    }
}
