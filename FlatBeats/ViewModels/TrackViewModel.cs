namespace FlatBeats.ViewModels
{
    using System;

    /// <summary>
    /// </summary>
    public class TrackViewModel : ViewModel
    {
        #region Public Properties

        /// <summary>
        /// </summary>
        public Uri AudioUrl { get; private set; }

        /// <summary>
        /// </summary>
        public string Title { get; private set; }

        #endregion
    }
}