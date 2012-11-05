namespace Flatliner.Phone.Controls
{
    using System;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Controls;

    public class ViewModelPage : PhoneApplicationPage
    {
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.NavigationParameters = this.NavigationContext.QueryString;
                this.ViewModel.State = this.State;
                this.Dispatcher.BeginInvoke(new Action(this.ViewModel.Load));
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.Unload();
            }

            base.OnNavigatedFrom(e);
        }

        protected PageViewModel ViewModel 
        { 
            get
            {
                return this.DataContext as PageViewModel;
            } 
        }
    }
}
