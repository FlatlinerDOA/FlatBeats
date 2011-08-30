namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;

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
