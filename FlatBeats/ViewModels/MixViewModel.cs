//--------------------------------------------------------------------------------------------------
// <copyright file="MixViewModel.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.ViewModels
{
    using System;
    using System.Linq;

    using FlatBeats.DataModel;

    /// <summary>
    /// </summary>
    public class MixViewModel : ViewModel, INavigationItem
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private string description;

        /// <summary>
        /// </summary>
        private Uri imageUrl;

        /// <summary>
        /// </summary>
        private string mixName;

        /// <summary>
        /// </summary>
        private Uri thumbnailUrl;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the MixViewModel class.
        /// </summary>
        public MixViewModel()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="mix">
        /// </param>
        public MixViewModel(MixContract mix)
        {
            this.MixName = mix.Name;
            var lines = mix.Description.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());
            this.Description = string.Join(Environment.NewLine, lines);
            this.ThumbnailUrl = mix.CoverUrls.ThumbnailUrl;
            this.ImageUrl = mix.CoverUrls.OriginalUrl;
            this.TileTitle = mix.Name.Replace(" ", Environment.NewLine);
            this.MixId = mix.Id;
            this.NavigationUrl = new Uri("/PlayPage.xaml?mix=" + this.MixId, UriKind.Relative);

            this.Tags = mix.Tags;
        }

        #endregion

        #region Public Properties

        public string MixId { get; private set; }

        /// <summary>
        /// </summary>
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                if (this.description == value)
                {
                    return;
                }

                this.description = value;
                this.OnPropertyChanged("Description");
            }
        }

        private string tags;

        public string Tags
        {
            get
            {
                return this.tags;
            }
            set
            {
                if (this.tags == value)
                {
                    return;
                }

                this.tags = value;
                this.OnPropertyChanged("Tags");
            }
        }

        /// <summary>
        /// </summary>
        public Uri ImageUrl
        {
            get
            {
                return this.imageUrl;
            }

            set
            {
                if (this.imageUrl == value)
                {
                    return;
                }

                this.imageUrl = value;
                this.OnPropertyChanged("ImageUrl");
            }
        }

        /// <summary>
        /// </summary>
        public string MixName
        {
            get
            {
                return this.mixName;
            }

            set
            {
                if (this.mixName == value)
                {
                    return;
                }

                this.mixName = value;
                this.OnPropertyChanged("MixName");
            }
        }

        private bool liked;

        public bool Liked
        {
            get
            {
                return this.liked;
            }
            set
            {
                if (this.liked == value)
                {
                    return;
                }

                this.liked = value;
                this.OnPropertyChanged("Liked");
            }
        }

        /// <summary>
        /// </summary>
        public Uri NavigationUrl { get; private set; }

        /// <summary>
        /// </summary>
        public Uri ThumbnailUrl
        {
            get
            {
                return this.thumbnailUrl;
            }

            set
            {
                if (this.thumbnailUrl == value)
                {
                    return;
                }

                this.thumbnailUrl = value;
                this.OnPropertyChanged("ThumbnailUrl");
            }
        }


        private string tileTitle;

        public string TileTitle
        {
            get
            {
                return this.tileTitle;
            }
            set
            {
                if (this.tileTitle == value)
                {
                    return;
                }

                this.tileTitle = value;
                this.OnPropertyChanged("TileTitle");
            }
        }
        #endregion
    }
}