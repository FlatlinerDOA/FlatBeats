
namespace FlatBeats.ViewModels
{
    using System;

    using FlatBeats.DataModel;
    using FlatBeats.Framework;

    using Flatliner.Phone.Data;

    public sealed class ReviewViewModel : ListItemViewModel, INavigationItem
    {
        private Uri avatarUrl;

        public Uri AvatarUrl
        {
            get
            {
                return this.avatarUrl;
            }
            set
            {
                if (this.avatarUrl == value)
                {
                    return;
                }

                this.avatarUrl = value;
                this.OnPropertyChanged("AvatarUrl");
            }
        }

        private string userName;

        public string UserName
        {
            get
            {
                return this.userName;
            }
            set
            {
                if (this.userName == value)
                {
                    return;
                }

                this.userName = value;
                this.OnPropertyChanged("UserName");
            }
        }

        private string body;

        public string Body
        {
            get
            {
                return this.body;
            }
            set
            {
                if (this.body == value)
                {
                    return;
                }

                this.body = value;
                this.OnPropertyChanged("Body");
            }
        }

        private DateTime created;

        /// <summary>
        /// Initializes a new instance of the ReviewViewModel class.
        /// </summary>
        public ReviewViewModel()
        {
        }

        public ReviewViewModel(ReviewContract review, bool censor)
        {
            this.Load(review, censor);
        }

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

        public Uri NavigationUrl
        {
            get; private set; }

        public void Load(ReviewContract review, bool censor)
        {
            if (review.User != null)
            {
                this.UserName = review.User.Name;
                this.AvatarUrl = Avatar.GetImageUrl(review.User.Avatar);
                this.NavigationUrl = PageUrl.UserProfile(review.User.Id);
            }

            var text = Html.ConvertToPlainText(review.Body).Trim();
            this.Body = censor ? Censorship.Censor(text) : text;
            DateTimeOffset createdDate;
            if (DateTimeOffset.TryParse(review.Created, out createdDate))
            {
                this.Created = createdDate.ToLocalTime().DateTime;
            } 
            else
            {
                this.Created = DateTime.Now.AddSeconds(-1);
            }

            if (this.Created > DateTime.Now)
            {
                this.Created = DateTime.Now.AddSeconds(-1);
            }
        }
    }
}
