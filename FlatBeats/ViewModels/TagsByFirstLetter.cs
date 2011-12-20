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
        private const string Groups = "#abcdefghijklmnopqrstuvwxyz";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public TagsByFirstLetter(IList<TagViewModel> tagViewModels)
        {
            List<TagViewModel> groupedTags = new List<TagViewModel>(tagViewModels);
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