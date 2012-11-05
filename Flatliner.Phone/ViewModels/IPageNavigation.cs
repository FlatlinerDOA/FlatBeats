namespace Flatliner.Phone.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Navigation;

    /// <summary>
    /// Inteface for a view model that is to be bound to a PhoneApplicationPage's DataContext and handles page navigation
    /// </summary>
    public interface IPageNavigation
    {
        NavigationService Navigation
        {
            get;
            set;
        }

        IDictionary<string, object> PageState
        {
            get;
            set;
        }

        void LoadPage(IDictionary<string, string> parameters);

        void RestorePageState();

        void SavePageState();

        void UnloadPage();
    }
}
