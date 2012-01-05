
namespace FlatBeats.ViewModels
{
    using System;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Flatliner.Phone.Data;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;

    public class ReviewViewModel : ViewModel, INavigationItem
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
            if (review.User != null)
            {
                this.UserName = review.User.Name;
                this.AvatarUrl = new Uri(review.User.Avatar.ImageUrl, UriKind.RelativeOrAbsolute);
                if (!this.AvatarUrl.IsAbsoluteUri)
                {
                    this.AvatarUrl = new Uri("http://8tracks.com" + review.User.Avatar.ImageUrl, UriKind.Absolute);
                }

                this.NavigationUrl = new Uri("/UserProfilePage.xaml?userid=" + review.User.Id, UriKind.Relative);
            }

            this.Body = Html.ConvertToPlainText(review.Body).Trim();
            this.Created = DateTimeOffset.Parse(review.Created).ToLocalTime().DateTime;
            if (this.Created > DateTime.Now)
            {
                this.Created = DateTime.Now.AddSeconds(-1);
            }
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
    }
}
