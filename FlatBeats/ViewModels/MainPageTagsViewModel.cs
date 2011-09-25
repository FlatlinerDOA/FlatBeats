namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;

    using Microsoft.Phone.Reactive;

    public class MainPageTagsViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainPageTagsViewModel class.
        /// </summary>
        public MainPageTagsViewModel()
        {
            this.Tags = new ObservableCollection<TagViewModel>();
            this.Title = StringResources.Title_Tags;
        }

        /// <summary>
        /// </summary>
        public ObservableCollection<TagViewModel> Tags { get; private set; }

        /// <summary>
        /// </summary>
        public void Load(IEnumerable<MixViewModel> mixes)
        {
            var load = from splitTags in Observable.Start(() => TagViewModel.SplitAndMergeIntoTags(mixes.Select(m => m.Tags)).OrderBy(t => t.TagName))
                       from t in splitTags.ToObservable()
                        select t;

            load.Concat(Observable.Return(new TagViewModel("more..."))).FlowIn()
                .ObserveOnDispatcher()
                .FirstDo(_ => this.Tags.Clear())
                .Subscribe(
                    t => this.Tags.Add(t), 
                    this.ShowError);
        }
    }
}
