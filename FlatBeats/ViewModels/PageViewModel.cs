namespace FlatBeats.ViewModels
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public abstract class PageViewModel : Flatliner.Phone.ViewModels.PageViewModel
    {
        /// <summary>
        /// Initializes a new instance of the PageViewModel class.
        /// </summary>
        protected PageViewModel()
        {
            this.RegisterErrorHandler<SocketException>(PanelViewModel.GetSocketErrorMessage);
            this.RegisterErrorHandler<WebException>(PanelViewModel.GetWebErrorMessage);
            this.RegisterErrorHandler<Exception>(PanelViewModel.GetUnknownErrorMessage);
        }
    }
}
