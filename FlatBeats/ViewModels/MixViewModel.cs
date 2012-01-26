// --------------------------------------------------------------------------------------------------------------------
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
    using System.Windows;

    using FlatBeats.DataModel;

    using Flatliner.Phone.Data;

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
            this.PlaysCountLabel = "plays";
            this.LikesCountLabel = "likes";
        }

        /// <summary>
        /// </summary>
        /// <param name="mix">
        /// </param>
        public MixViewModel(MixContract mix) : this()
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
                if (this.tagList == null && this.Tags != null)
                {
                    this.tagList = this.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(
        t => !string.IsNullOrWhiteSpace(t)).Select(t => new TagViewModel(t.Trim())).ToList();
                }

                return this.tagList;
            }
        }

        private int likesCount;

        public int LikesCount
        {
            get
            {
                return this.likesCount;
            }
            set
            {
                if (this.likesCount == value)
                {
                    return;
                }

                this.likesCount = value;
                this.OnPropertyChanged("LikesCount");
            }
        }

        private int playsCount;

        public int PlaysCount
        {
            get
            {
                return this.playsCount;
            }
            set
            {
                if (this.playsCount == value)
                {
                    return;
                }

                this.playsCount = value;
                this.OnPropertyChanged("PlaysCount");
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
                this.tagList = null;
                this.OnPropertyChanged("Tags");
                this.OnPropertyChanged("TagList");
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

        private string playsCountLabel;

        public string PlaysCountLabel
        {
            get
            {
                return this.playsCountLabel;
            }
            set
            {
                if (this.playsCountLabel == value)
                {
                    return;
                }

                this.playsCountLabel = value;
                this.OnPropertyChanged(() => this.PlaysCountLabel);
            }
        }

        private string likesCountLabel;

        public string LikesCountLabel
        {
            get
            {
                return this.likesCountLabel;
            }
            set
            {
                if (this.likesCountLabel == value)
                {
                    return;
                }

                this.likesCountLabel = value;
                this.OnPropertyChanged(() => this.LikesCountLabel);
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

        private bool isExplicit;

        public bool IsExplicit
        {
            get
            {
                return this.isExplicit;
            }
            set
            {
                if (this.isExplicit == value)
                {
                    return;
                }

                this.isExplicit = value;
                this.OnPropertyChanged("IsExplicit");
            }
        }

        #endregion

        public void Load(MixContract mix)
        {
            this.IsExplicit = mix.IsExplicit && ((App)Application.Current).UserSettings.CensorshipEnabled;
            this.MixName = this.IsExplicit ? Censorship.Censor(mix.Name) : mix.Name;
            var lines =
                mix.Description.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t));
            this.Description = string.Join(Environment.NewLine, lines);
            this.ThumbnailUrl = this.IsExplicit ? this.AddQuery(mix.Cover.ThumbnailUrl, "nsfw") : mix.Cover.ThumbnailUrl;
            this.ImageUrl = this.IsExplicit ? this.AddQuery(mix.Cover.OriginalUrl, "nsfw") : mix.Cover.OriginalUrl;
            this.TileTitle = mix.Name.Replace(" ", Environment.NewLine);
            this.MixId = mix.Id;
            this.NavigationUrl = new Uri("/PlayPage.xaml?mix=" + this.MixId, UriKind.Relative);
            this.LinkUrl = new Uri(mix.RestUrl, UriKind.RelativeOrAbsolute);
            this.Liked = mix.Liked;
            this.CreatedBy = mix.User.Name;
            this.CreatedByAvatarUrl = new Uri(mix.User.Avatar.ImageUrl, UriKind.RelativeOrAbsolute);
            this.Created = DateTimeOffset.Parse(mix.Created).ToLocalTime().DateTime;
            this.PlaysCount = mix.PlaysCount;
            this.LikesCount = mix.LikesCount;

            if (this.Created > DateTime.Now)
            {
                this.Created = DateTime.Now.AddSeconds(-1);
            }

            this.Tags = mix.Tags;

        }

        private Uri AddQuery(Uri uri, string value)
        {
            var newUrl = new UriBuilder(uri);
            if (newUrl.Query.Length == 0)
            {
                newUrl.Query = value;
            } 
            else
            {
                newUrl.Query += "&" + value;
            }

            return newUrl.Uri;
        }
    }
}