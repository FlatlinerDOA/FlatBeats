﻿// --------------------------------------------------------------------------------------------------------------------
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
        private readonly string userId;

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
        /// </summary>
        /// <param name="data">
        /// </param>
        public UserListItemViewModel(UserContract data)
        {
            this.userId = data.Id;
            this.Name = data.Name;
            this.Location = data.Location;
            this.Bio = Html.ConvertToPlainText(data.BioHtml);

            Uri url;
            if (Uri.TryCreate(data.Avatar.ImageUrl, UriKind.RelativeOrAbsolute, out url))
            {
                this.AvatarUrl = url;
            }
            else
            {
                this.AvatarUrl = null;
            }

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
            get
            {
                return new Uri("/UserProfilePage.xaml?userid=" + this.userId);
            }
        }
    }
}