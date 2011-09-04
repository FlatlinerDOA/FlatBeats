namespace FlatBeats
{
    using System;
    using System.Linq;

    using FlatBeats.Controls;
    using FlatBeats.ViewModels;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;

    using NavigationEventArgs = System.Windows.Navigation.NavigationEventArgs;

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
            ApplicationBarBinder.Bind(this, this.ViewModel);
            this.ViewModel.MixId = this.NavigationContext.QueryString["mix"];
            this.ViewModel.Load();
            ////this.ViewModel.PlayStates.ObserveOnDispatcher().Subscribe(this.UpdatePlayState);
        }

        private void UpdatePlayState(bool canPause)
        {
            var button = this.ApplicationBar.Buttons.OfType<ApplicationBarIconButton>().FirstOrDefault();
            if (button != null)
            {
                if (canPause)
                {
                    button.IconUri = new Uri("/icons/appbar.transport.pause.rest.png", UriKind.Relative);
                    button.Text = "pause";
                }
                else
                {
                    button.IconUri = new Uri("/icons/appbar.transport.play.rest.png", UriKind.Relative);
                    button.Text = "play";
                }
            }
        }

        #endregion

        private void Share_Click(object sender, EventArgs e)
        {
            this.ViewModel.Share();
        }

        private void Play_Click(object sender, EventArgs e)
        {
            this.ViewModel.Play();
        }

        private void Email_Click(object sender, EventArgs e)
        {
            this.ViewModel.Email();
        }
    }
}