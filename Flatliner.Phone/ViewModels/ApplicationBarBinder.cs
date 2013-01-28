namespace Flatliner.Phone.ViewModels
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Reactive;
    using Microsoft.Phone.Shell;

    public class ApplicationBarBinder : IDisposable
    {
        private readonly CompositeDisposable appBarSubscriptions = new CompositeDisposable();

        /// <summary>
        /// Initializes a new instance of the ApplicationBarBinder class.
        /// </summary>
        public ApplicationBarBinder(PhoneApplicationPage page)
        {
            this.Page = page;
            this.Page.ApplicationBar.StateChanged += this.UpdateSystemTray;
            if (this.ShowSystemTrayWhenMenuOpens)
            {
                SystemTray.SetIsVisible(this.Page, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateSystemTray(object sender, ApplicationBarStateChangedEventArgs e)
        {
            if (this.ShowSystemTrayWhenMenuOpens)
            {
                SystemTray.SetIsVisible(this.Page, e.IsMenuVisible);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ApplicationBarBinder class.
        /// </summary>
        public ApplicationBarBinder(PhoneApplicationPage page, IApplicationBarViewModel viewModel)
            : this(page)
        {
            this.BindApplicationBar(viewModel);
        }

        protected PhoneApplicationPage Page
        {
            get;
            private set;
        }

        public bool ShowSystemTrayWhenMenuOpens
        {
            get;
            set;
        }

        private void RefreshButtons(IApplicationBarViewModel viewModel)
        {
            if (this.Page == null || this.Page.ApplicationBar == null)
            {
                return;
            }

            this.Page.ApplicationBar.Buttons.Clear();
            foreach (var commandLink in viewModel.ApplicationBarButtonCommands)
            {
                this.AddApplicationBarButton(commandLink);
            }
        }

        private void RefreshMenuItems(IApplicationBarViewModel appBarViewModel)
        {
            if (this.Page == null || this.Page.ApplicationBar == null)
            {
                return;
            }

            this.Page.ApplicationBar.MenuItems.Clear();
            foreach (var commandLink in appBarViewModel.ApplicationBarMenuCommands)
            {
                this.AddApplicationBarMenu(commandLink);
            }
        }

        public void BindApplicationBar(IApplicationBarViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            if (this.Page == null || this.Page.ApplicationBar == null)
            {
                return;
            }

            if (this.Page.ApplicationBar.IsMenuEnabled)
            {
                var appBarViewModel = (IApplicationBarViewModel)viewModel;
                this.RefreshButtons(appBarViewModel);
                this.appBarSubscriptions.Add(
                    Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        handler => new NotifyCollectionChangedEventHandler(handler),
                        handler => appBarViewModel.ApplicationBarButtonCommands.CollectionChanged += handler,
                        handler => appBarViewModel.ApplicationBarButtonCommands.CollectionChanged -= handler).
                        ObserveOnDispatcher().Subscribe(_ => this.RefreshButtons(appBarViewModel)));

                this.RefreshMenuItems(appBarViewModel);
                this.appBarSubscriptions.Add(
                    Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        handler => new NotifyCollectionChangedEventHandler(handler),
                        handler => appBarViewModel.ApplicationBarMenuCommands.CollectionChanged += handler,
                        handler => appBarViewModel.ApplicationBarMenuCommands.CollectionChanged -= handler).
                        ObserveOnDispatcher().Subscribe(_ => this.RefreshMenuItems(appBarViewModel)));
            }
        }

        private void AddApplicationBarButton(ICommandLink commandLink)
        {
            if (this.Page.ApplicationBar == null)
            {
                return;
            }

            bool canExecute = commandLink.Command != null && commandLink.Command.CanExecute(null);
            var button = new ApplicationBarIconButton
            {
                IconUri = commandLink.IconUrl,
                Text = commandLink.Text,
                IsEnabled = canExecute
            };

            if (commandLink.Command != null)
            {
                this.appBarSubscriptions.Add(
                    Observable.FromEvent<EventHandler, EventArgs>(
                        handler => new EventHandler(handler),
                        handler => button.Click += handler,
                        handler => button.Click -= handler).ObserveOnDispatcher().Subscribe(
                            _ => commandLink.Command.Execute(null)));
                this.appBarSubscriptions.Add(
                    Observable.FromEvent<EventArgs>(commandLink.Command, "CanExecuteChanged").Subscribe(
                        ev => this.UpdateButtonCanExecute(commandLink, button)));
            }

            if (canExecute || !commandLink.HideWhenInactive)
            {
                this.Page.ApplicationBar.Buttons.Add(button);
            }
        }


        private void UpdateButtonCanExecute(ICommandLink commandLink, ApplicationBarIconButton button)
        {
            if (commandLink == null || commandLink.Command == null)
            {
                return;
            }

            bool canExecute =  commandLink.Command.CanExecute(null);
            button.IsEnabled = canExecute;
            button.IconUri = commandLink.IconUrl;
            button.Text = commandLink.Text;
            if (!commandLink.HideWhenInactive)
            {
                return;
            }

            if (this.Page == null || 
                this.Page.ApplicationBar == null || 
                this.Page.ApplicationBar.Buttons == null || 
                canExecute == this.Page.ApplicationBar.Buttons.Contains(button))
            {
                return;
            }

            if (canExecute)
            {
                this.Page.ApplicationBar.Buttons.Add(button);
            }
            else
            {
                this.Page.ApplicationBar.Buttons.Remove(button);
            }
        }

        private void UpdateMenuItemCanExecute(ICommandLink commandLink, ApplicationBarMenuItem button)
        {
            if (commandLink == null || commandLink.Command == null)
            {
                return;
            }

            bool canExecute = commandLink.Command.CanExecute(null);
            button.IsEnabled = canExecute;
            button.Text = commandLink.Text;
            if (!commandLink.HideWhenInactive)
            {
                return;
            }

            if (this.Page == null ||
                this.Page.ApplicationBar == null ||
                this.Page.ApplicationBar.MenuItems == null || 
                canExecute == this.Page.ApplicationBar.MenuItems.Contains(button))
            {
                return;
            }

            if (canExecute)
            {
                this.Page.ApplicationBar.MenuItems.Add(button);
            }
            else
            {
                this.Page.ApplicationBar.MenuItems.Remove(button);
            }
        }

        private void AddApplicationBarMenu(ICommandLink commandLink)
        {
            if (this.Page.ApplicationBar == null)
            {
                return;
            }

            bool canExecute = commandLink.Command != null && commandLink.Command.CanExecute(null);
            var button = new ApplicationBarMenuItem() { Text = commandLink.Text, IsEnabled = canExecute };
            if (commandLink.Command != null)
            {
                this.appBarSubscriptions.Add(
                    Observable.FromEvent<EventHandler, EventArgs>(
                        handler => new EventHandler(handler),
                        handler => button.Click += handler,
                        handler => button.Click -= handler).ObserveOnDispatcher().Subscribe(
                            _ => commandLink.Command.Execute(null)));
                this.appBarSubscriptions.Add(
                    Observable.FromEvent<EventArgs>(commandLink.Command, "CanExecuteChanged").Subscribe(
                        ev => this.UpdateMenuItemCanExecute(commandLink, button)));
                this.appBarSubscriptions.Add(
                    Observable.FromEvent<PropertyChangedEventArgs>(commandLink, "PropertyChanged").Subscribe(
                        ev => button.Text = commandLink.Text));
            }

            if (canExecute || !commandLink.HideWhenInactive)
            {
                this.Page.ApplicationBar.MenuItems.Add(button);
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.Page != null && this.Page.ApplicationBar != null)
            {
                this.Page.ApplicationBar.StateChanged -= this.UpdateSystemTray;
            }

            this.appBarSubscriptions.Dispose();
        }
    }
}
