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
            this.RegisterErrorHandler<SocketException>(ex => StringResources.Error_NoNetwork);
            this.RegisterErrorHandler<WebException>(PanelViewModel.GetWebExceptionMessage);
            this.RegisterErrorHandler<Exception>(ex => StringResources.Error_UnknownError);
        }
    }

    ////public abstract class PageViewModel : PanelViewModel
    ////{
    ////    /// <summary>
    ////    /// Initializes a new instance of the PageViewModel class.
    ////    /// </summary>
    ////    protected PageViewModel()
    ////    {
    ////        this.NavigationParameters = new Dictionary<string, string>(2);
    ////    }

    ////    private bool isInProgress;

    ////    public bool IsInProgress
    ////    {
    ////        get
    ////        {
    ////            return this.isInProgress;
    ////        }

    ////        private set
    ////        {
    ////            if (this.isInProgress == value)
    ////            {
    ////                return;
    ////            }

    ////            this.isInProgress = value;
    ////            this.OnPropertyChanged("IsInProgress");
    ////        }
    ////    }

    ////    public bool IsDataLoaded { get; private set; }

    ////    public override void ShowError(Exception error)
    ////    {
    ////        this.HideProgress();
    ////        var message = this.GetMessageForException(error);
    ////        if (message != null)
    ////        {
    ////            MessageBox.Show(message, StringResources.Error_Title, MessageBoxButton.OK);
    ////        }

    ////        this.Message = message;
    ////        this.ShowMessage = true;
    ////    }

    ////    public IDictionary<string, string> NavigationParameters { get; private set; }

    ////    public abstract void Load();

    ////    protected void LoadCompleted()
    ////    {
    ////        this.HideProgress();
    ////        this.IsDataLoaded = true;
    ////    }

    ////    protected void ShowProgress()
    ////    {
    ////        this.IsInProgress = true;
    ////    }

    ////    protected void HideProgress()
    ////    {
    ////        this.IsInProgress = false;
    ////    }
    ////}
}
