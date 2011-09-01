namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Reactive;

    public class TagsPageViewModel : ViewModel
    {
        /// <summary>
        /// Initializes a new instance of the TagsPageViewModel class.
        /// </summary>
        public TagsPageViewModel()
        {
            this.Tags = new ObservableCollection<TagViewModel>();
        }

        public void Load()
        {
            this.IsInProgress = true;
            var list = new List<TagViewModel>();
            var tags = from page in Observable.Range(1, 10)
                       from response in Downloader.GetJson<TagsResponseContract>(new Uri("http://8tracks.com/tags.json?page=" + page))
                       from tag in response.Tags.ToObservable()
                       select new TagViewModel(tag.Name);
            tags.ObserveOnDispatcher().Subscribe(list.Add, this.ShowError, () =>
                {
                    foreach (var tag in list.OrderBy(tag => tag.TagName))
                    {
                        this.Tags.Add(tag);
                    }

                    this.HideProgress();
                });
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        private void HideProgress()
        {
            this.IsInProgress = false;
        }

        public ObservableCollection<TagViewModel> Tags { get; set; }

        private string title;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (this.title == value)
                {
                    return;
                }

                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        private bool isInProgress;

        public bool IsInProgress
        {
            get
            {
                return this.isInProgress;
            }
            set
            {
                if (this.isInProgress == value)
                {
                    return;
                }

                this.isInProgress = value;
                this.OnPropertyChanged("IsInProgress");
            }
        }
    }
}
