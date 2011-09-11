
namespace FlatBeats.ViewModels
{
    using System;

    using FlatBeats.DataModel;

    public class ReviewViewModel : ViewModel
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

        public ReviewViewModel(ReviewContract review)
        {
            this.UserName = review.User.Name;
            this.Body = review.Body.Trim();
            this.AvatarUrl = new Uri(review.User.Avatar.ImageUrl, UriKind.RelativeOrAbsolute);
            if (!this.AvatarUrl.IsAbsoluteUri)
            {
                this.AvatarUrl = new Uri("http://8tracks.com" + review.User.Avatar.ImageUrl, UriKind.Absolute);
            }

            this.Created = DateTimeOffset.Parse(review.Created).ToLocalTime().DateTime;
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
    }
}
