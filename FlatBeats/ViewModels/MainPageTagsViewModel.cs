namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using FlatBeats.Framework;

    using Flatliner.Phone;

    public sealed class MainPageTagsViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainPageTagsViewModel class.
        /// </summary>
        public MainPageTagsViewModel()
        {
            this.Tags = new ObservableCollection<TagViewModel>();
            this.Title = StringResources.Title_Tags;
            this.Message = StringResources.Progress_Loading;
        }

        /// <summary>
        /// </summary>
        public ObservableCollection<TagViewModel> Tags { get; private set; }

        /// <summary>
        /// </summary>
        public void Load(IEnumerable<MixViewModel> mixes)
        {
            var tagList = TagViewModel.SplitAndMergeIntoTags(mixes.Select(m => m.Tags).NotNullOrEmpty()).OrderBy(t => t.TagName).ToList();
            tagList.Add(new TagViewModel("more..."));

            if (tagList.Count == 0)
            {
                this.Message = StringResources.Error_NoNetwork_Message;
            }
            else
            {
                this.Message = null;
            }

            this.Tags.Clear();
            foreach (var tagViewModel in tagList.SetListItemPositions())
            {
                this.Tags.Add(tagViewModel);
            }
        }
    }
}
