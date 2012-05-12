namespace FlatBeats.ViewModels
{
    using System.Collections.Generic;

    /// <summary>
    /// </summary>
    public sealed class TagsInGroup : List<TagViewModel>
    {
        #region Constructors and Destructors

        public TagsInGroup()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="category">
        /// </param>
        public TagsInGroup(string category)
        {
            this.Key = category;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public bool HasItems
        {
            get
            {
                return this.Count > 0;
            }
        }

        /// <summary>
        /// </summary>
        public string Key { get; set; }

        #endregion
    }
}