namespace Flatliner.Phone.ViewModels
{
    using System;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Tasks;

    public class PhotoChooserBinder : IDisposable
    {
        #region Constants and Fields

        private readonly IDisposable subscription;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the PhotoChooserBinder class.
        /// </summary>
        public PhotoChooserBinder(IPhotoChooserViewModel chooser)
        {
            this.Chooser = chooser;
            this.Task = new PhotoChooserTask();
            this.Task.Completed += this.PhotoSelected;
            this.subscription = this.Chooser.PhotoRequests.Subscribe(this.PhotoRequested);
        }

        #endregion

        public IPhotoChooserViewModel Chooser
        {
            get;
            private set;
        }

        protected PhotoChooserTask Task
        {
            get;
            set;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Task.Completed -= this.PhotoSelected;
            this.subscription.Dispose();
        }

        private void PhotoRequested(PhotoRequest request)
        {
            this.Task.PixelWidth = request.Width;
            this.Task.PixelHeight = request.Height;
            this.Task.ShowCamera = request.ShowCamera;
            this.Task.Show();
        }

        private void PhotoSelected(object sender, PhotoResult e)
        {
            this.Chooser.ProcessResult(e);
        }
    }
}