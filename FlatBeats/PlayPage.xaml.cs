namespace FlatBeats
{
    using System;
    using System.Windows.Navigation;

    using FlatBeats.ViewModels;

    using Microsoft.Phone.Controls;

    /// <summary>
    /// </summary>
    public partial class PlayPage : PhoneApplicationPage
    {
        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public PlayPage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public PlayPageViewModel ViewModel
        {
            get
            {
                return this.DataContext as PlayPageViewModel;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="e">
        /// </param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.MixId = this.NavigationContext.QueryString["mix"];
            this.ViewModel.Load();
        }

        #endregion
    }
}