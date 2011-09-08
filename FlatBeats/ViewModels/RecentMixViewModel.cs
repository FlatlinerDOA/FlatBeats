namespace FlatBeats.ViewModels
{
    using FlatBeats.DataModel;

    public class RecentMixViewModel : MixViewModel
    {
        /// <summary>
        /// Initializes a new instance of the RecentMixViewModel class.
        /// </summary>
        public RecentMixViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RecentMixViewModel class.
        /// </summary>
        public RecentMixViewModel(MixContract mix) : base(mix)
        {
        }

        private bool isNowPlaying;

        public bool IsNowPlaying
        {
            get
            {
                return this.isNowPlaying;
            }
            set
            {
                if (this.isNowPlaying == value)
                {
                    return;
                }

                this.isNowPlaying = value;
                this.OnPropertyChanged("IsNowPlaying");
            }
        }
    }
}