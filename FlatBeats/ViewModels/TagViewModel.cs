// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TagViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// </summary>
    public class TagViewModel : ListItemViewModel, INavigationItem
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private string tagName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the TagViewModel class.
        /// </summary>
        /// <param name="name">
        /// </param>
        public TagViewModel(string name)
        {
            this.TagName = name;
            if (!string.IsNullOrWhiteSpace(this.TagName))
            {
                this.Key = this.TagName.Trim().ToLowerInvariant().First().ToString();
            }

            if (!char.IsLetter(this.Key[0]))
            {
                this.Key = "#";
            }

            if (this.TagName == "more...")
            {
                this.IsLastItem = true;
                this.NavigationUrl = new Uri("/TagsPage.xaml", UriKind.Relative);
            }
            else
            {
                this.NavigationUrl = new Uri("/MixesPage.xaml?tag=" + this.TagName, UriKind.Relative);
            }
        }

        /// <summary>
        ///   Initializes a new instance of the TagViewModel class.
        /// </summary>
        public TagViewModel()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public Uri NavigationUrl { get; private set; }

        public string Key { get; private set; }

        /// <summary>
        /// </summary>
        public string TagName
        {
            get
            {
                return this.tagName;
            }

            set
            {
                if (this.tagName == value)
                {
                    return;
                }

                this.tagName = value;
                this.OnPropertyChanged("TagName");
            }
        }

   

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="tags">
        /// </param>
        /// <returns>
        /// </returns>
        public static IEnumerable<TagViewModel> SplitAndMergeIntoTags(IEnumerable<string> tags)
        {
            var allTags = (from tagList in tags
                           from tag in tagList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                           let cleanedTag = tag.Trim().ToLower()
                           where !string.IsNullOrWhiteSpace(cleanedTag)
                           select cleanedTag).Distinct();
            return allTags.Select(tag => new TagViewModel(tag));
        }

        #endregion
    }
}