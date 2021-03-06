﻿namespace FlatBeats.ViewModels
{
    using FlatBeats.DataModel;
    using FlatBeats.Framework;

    public sealed class RecentMixViewModel : MixViewModel
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
        public RecentMixViewModel(MixContract mix, bool censor)
            : base(mix, censor)
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