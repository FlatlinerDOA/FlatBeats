﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MixViewModel.cs" company="">
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
    using System.Linq;

    using FlatBeats.DataModel;

    /// <summary>
    /// </summary>
    public class MixViewModel : ListItemViewModel, INavigationItem
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private DateTime created;

        /// <summary>
        /// </summary>
        private string createdBy;

        /// <summary>
        /// </summary>
        private Uri createdByAvatarUrl;

        /// <summary>
        /// </summary>
        private string description;

        /// <summary>
        /// </summary>
        private Uri imageUrl;

        /// <summary>
        /// </summary>
        private bool liked;

        /// <summary>
        /// </summary>
        private string mixName;

        /// <summary>
        /// </summary>
        private List<TagViewModel> tagList;

        /// <summary>
        /// </summary>
        private string tags;

        /// <summary>
        /// </summary>
        private Uri thumbnailUrl;

        /// <summary>
        /// </summary>
        private string tileTitle;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the MixViewModel class.
        /// </summary>
        public MixViewModel()
        {
            this.MixName = string.Empty;
            this.Description = string.Empty;
            this.TileTitle = string.Empty;
        }

        /// <summary>
        /// </summary>
        /// <param name="mix">
        /// </param>
        public MixViewModel(MixContract mix)
        {
            this.Load(mix);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public DateTime Created
        {
            get
            {
                return this.created;
            }

            set
            {
                if (this.created == value)
                {
                    return;
                }

                this.created = value;
                this.OnPropertyChanged("Created");
            }
        }

        /// <summary>
        /// </summary>
        public string CreatedBy
        {
            get
            {
                return this.createdBy;
            }

            set
            {
                if (this.createdBy == value)
                {
                    return;
                }

                this.createdBy = value;
                this.OnPropertyChanged("CreatedBy");
            }
        }

        /// <summary>
        /// </summary>
        public Uri CreatedByAvatarUrl
        {
            get
            {
                return this.createdByAvatarUrl;
            }

            set
            {
                if (this.createdByAvatarUrl == value)
                {
                    return;
                }

                this.createdByAvatarUrl = value;
                this.OnPropertyChanged("CreatedByAvatarUrl");
            }
        }

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
        public Uri LinkUrl { get; private set; }

        /// <summary>
        /// </summary>
        public string MixId { get; private set; }

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

        /// <summary>
        /// </summary>
        public Uri NavigationUrl { get; set; }

        /// <summary>
        /// </summary>
        public List<TagViewModel> TagList
        {
            get
            {
                return this.tagList;
            }

            set
            {
                if (this.tagList == value)
                {
                    return;
                }

                this.tagList = value;
                this.OnPropertyChanged("TagList");
            }
        }

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
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

        public void Load(MixContract mix)
        {
            this.MixName = mix.Name;
            var lines =
                mix.Description.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t));
            this.Description = string.Join(Environment.NewLine, lines);
            this.ThumbnailUrl = mix.Cover.ThumbnailUrl;
            this.ImageUrl = mix.Cover.OriginalUrl;
            this.TileTitle = mix.Name.Replace(" ", Environment.NewLine);
            this.MixId = mix.Id;
            this.NavigationUrl = new Uri("/PlayPage.xaml?mix=" + this.MixId, UriKind.Relative);
            this.LinkUrl = new Uri(mix.RestUrl, UriKind.RelativeOrAbsolute);
            this.Liked = mix.Liked;
            this.CreatedBy = mix.User.Name;
            this.CreatedByAvatarUrl = new Uri(mix.User.Avatar.ImageUrl, UriKind.RelativeOrAbsolute);
            this.Created = DateTimeOffset.Parse(mix.Created).ToLocalTime().DateTime;
            if (this.Created > DateTime.Now)
            {
                this.Created = DateTime.Now.AddSeconds(-1);
            }

            this.Tags = mix.Tags;
            this.TagList =
                this.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(
                    t => !string.IsNullOrWhiteSpace(t)).Select(t => new TagViewModel(t.Trim())).ToList();

        }
    }
}