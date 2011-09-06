namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public class TagsByFirstLetter : List<TagsInGroup>
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private static readonly string Groups = "#abcdefghijklmnopqrstuvwxyz";

        #endregion

        ////private Dictionary<int, TagViewModel> _personLookup = new Dictionary<int, TagViewModel>();
        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public TagsByFirstLetter()
        {
            var list = new List<TagViewModel>();

            var tagViewModels = from page in Observable.Range(1, 4)
                                from response in
                                    Downloader.GetJson<TagsResponseContract>(
                                        new Uri("http://8tracks.com/all/mixes/tags.json?sort=recent&tag_page=" + page))
                                from tag in response.Tags.ToObservable()
                                select new TagViewModel(tag.Name);

            List<TagViewModel> groupedTags = new List<TagViewModel>(tagViewModels.ToEnumerable().ToList());
            groupedTags.Sort(
                (a, b) => string.Compare(a.TagName.Trim(), b.TagName.Trim(), StringComparison.OrdinalIgnoreCase));

            Dictionary<string, TagsInGroup> groups = new Dictionary<string, TagsInGroup>();

            foreach (char c in Groups)
            {
                TagsInGroup group = new TagsInGroup(c.ToString());
                this.Add(group);
                groups[c.ToString()] = group;
            }

            foreach (TagViewModel groupedTag in groupedTags)
            {
                groups[groupedTag.Key].Add(groupedTag);
            }
        }

        #endregion
    }
}