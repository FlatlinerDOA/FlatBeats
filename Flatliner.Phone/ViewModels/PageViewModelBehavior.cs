namespace Flatliner.Phone.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interactivity;
    using System.Windows.Navigation;
    using Flatliner.Phone.Extensions;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Reactive;

    public class PageViewModelBehavior : Behavior<PhoneApplicationPage>
    {
        #region Constants and Fields

        private ApplicationBarBinder appBarBinder;

        private PhotoChooserBinder photoChooserBinder;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the PageViewModelBehavior class.
        /// </summary>
        public PageViewModelBehavior()
        {
            this.ShowSystemTrayWhenMenuOpens = true;
        }

        #endregion

        protected bool IsPageActivated
        {
            get;
            set;
        }

        private PhoneApplicationPage Page
        {
            get
            {
                return this.AssociatedObject;
            }
        }

        private IPageNavigation PageViewModel
        {
            get;
            set;
        }

        public bool ShowSystemTrayWhenMenuOpens
        {
            get;
            set;
        }

        protected override void OnAttached()
        {
            Debug.WriteLine("PageViewModelBehavior: Attached");
            this.AssociatedObject.LayoutUpdated += this.PageLayoutUpdated;
            this.AssociatedObject.Loaded += this.PageLoaded;
            this.AssociatedObject.OrientationChanged += this.PageOrientationChanged;
            this.IsPageActivated = true;
            base.OnAttached();
        }

        /// <summary>
        /// Always called even on back navigations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Page.Loaded");
            this.appBarBinder = new ApplicationBarBinder(this.Page, this.PageViewModel as IApplicationBarViewModel)
                {
                    ShowSystemTrayWhenMenuOpens = this.ShowSystemTrayWhenMenuOpens
                };
        }

        protected override void OnDetaching()
        {
            Debug.WriteLine("PageViewModelBehavior: Detaching");
            this.AssociatedObject.Loaded -= this.PageLoaded;
            this.AssociatedObject.LayoutUpdated -= this.PageLayoutUpdated;
            this.AssociatedObject.OrientationChanged -= this.PageOrientationChanged;
            this.UnloadPage(null);
            base.OnDetaching();
        }

        /// <summary>
        /// Called when loading a page for the first time (does not get called on back navigations returning to this page)
        /// </summary>
        private void LoadPage()
        {
            this.PageViewModel = this.AssociatedObject.DataContext as IPageNavigation;
            if (this.PageViewModel == null)
            {
                this.PageViewModel =
                    this.Page.GetLogicalChildren().Select(d => d.DataContext).OfType<IPageNavigation>().FirstOrDefault();
                if (this.PageViewModel == null)
                {
                    return;
                }
            }

            this.PageNavigatedTo();
        }

        private void PageLayoutUpdated(object sender, EventArgs e)
        {
            Debug.WriteLine("PageViewModelBehavior: PageLayoutUpdated");
            this.AssociatedObject.LayoutUpdated -= this.PageLayoutUpdated;
            this.LoadPage();
        }

        private void PageNavigatedTo()
        {
            if (ApplicationState.OpenState == ApplicationOpenState.Activated && !this.IsPageActivated)
            {
                // We are returning to the application, but we were not tombstoned.
                ////ApplicationState.AddTimeLog(new TimeLog("Main Page Nav.To Done. No TS"));
                ApplicationState.OpenState = ApplicationOpenState.None;
                return;
            }

            try
            {
                this.PageViewModel.PageState = this.Page.State;
            }
            catch (InvalidOperationException)
            {
                // HACK: Way of telling that the page hasn't been navigated to yet.
                return;
            }

            if (this.PageViewModel is IPhotoChooserViewModel)
            {
                this.photoChooserBinder = new PhotoChooserBinder((IPhotoChooserViewModel)this.PageViewModel);
            }

            this.PageViewModel.Navigation = this.Page.NavigationService;
            Dictionary<string, string> navigationParameters;
            if (this.PageViewModel.Navigation != null)
            {
                var q =
                    from queryString in this.PageViewModel.Navigation.CurrentSource.OriginalString.Split('?').Skip(1)
                    from keyValue in queryString.Split('&')
                    let parts = keyValue.Split('=')
                    select new KeyValuePair<string, string>(parts[0], parts[1]);
                navigationParameters = q.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

                var navigating =
                    Observable.FromEvent<NavigationEventArgs>(
                        handler => this.Page.NavigationService.Navigated += new NavigatedEventHandler(handler),
                        handler => this.Page.NavigationService.Navigated -= new NavigatedEventHandler(handler));
                navigating.Take(1).Subscribe(this.UnloadPage);

            }
            else
            {
                navigationParameters = new Dictionary<string, string>();
            }

            this.PageViewModel.LoadPage(navigationParameters);
        }

        private void PageOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            VisualStateManager.GoToState(this.Page, this.Page.Orientation.ToString(), true);
        }

        private void UnloadPage(IEvent<NavigationEventArgs> navigation)
        {
            Debug.WriteLine("PageViewModelBehavior: UnloadPage (" + ApplicationState.OpenState + ")");
            this.IsPageActivated = false;
#if MANGO
            if (navigation.EventArgs.NavigationMode != NavigationMode.Back)
            {
                this.PageViewModel.SavePageState();
            }
#else
            this.PageViewModel.SavePageState();
#endif

            this.PageViewModel.UnloadPage();
            if (this.Page.ApplicationBar != null)
            {
                this.Page.ApplicationBar.Buttons.Clear();
                this.Page.ApplicationBar.MenuItems.Clear();
            }

            this.appBarBinder.Dispose();
        }
    }
}