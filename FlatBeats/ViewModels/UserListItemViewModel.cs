// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserListItemViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.ViewModels
{
    using System;

    using FlatBeats.DataModel;

    using Flatliner.Phone.Data;

    /// <summary>
    /// </summary>
    public class UserListItemViewModel : ListItemViewModel, INavigationItem
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private string userId;

        /// <summary>
        /// </summary>
        private Uri avatarUrl;

        /// <summary>
        /// </summary>
        private string bio;

        /// <summary>
        /// </summary>
        private string location;

        /// <summary>
        /// </summary>
        private string name;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the UserListItemViewModel class.
        /// </summary>
        public UserListItemViewModel()
        {
            
        }

        /// <summary>
        /// </summary>
        /// <param name="data">
        /// </param>
        public UserListItemViewModel(UserContract data)
        {
            this.Load(data);
        }

        private void Load(UserContract data)
        {
            this.userId = data.Id;
            this.Name = data.Name;
            this.Location = data.Location;
            this.Bio = Html.ConvertToPlainText(data.BioHtml);
            this.AvatarUrl = Avatar.ParseUrl(data.Avatar.ImageUrl);
            this.NavigationUrl = new Uri("/UserProfilePage.xaml?userid=" + this.userId, UriKind.Relative);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
        public string Bio
        {
            get
            {
                return this.bio;
            }

            set
            {
                if (this.bio == value)
                {
                    return;
                }

                this.bio = value;
                this.OnPropertyChanged("Bio");
            }
        }

        /// <summary>
        /// </summary>
        public string Location
        {
            get
            {
                return this.location;
            }

            set
            {
                if (this.location == value)
                {
                    return;
                }

                this.location = value;
                this.OnPropertyChanged("Location");
            }
        }

        /// <summary>
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (this.name == value)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged("Name");
            }
        }

        #endregion

        public Uri NavigationUrl
        {
            get; private set; }
    }
}