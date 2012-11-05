namespace Flatliner.Phone.Behaviors
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interactivity;
    using System.Windows.Navigation;
    using Flatliner.Phone.Extensions;
    using Microsoft.Phone.Controls;

    public class AnimatePageBehavior : Behavior<PhoneApplicationPage>
    {
        #region Constants and Fields

        private static readonly Uri ExternalUri = new Uri(@"app://external/");

        private static Uri fromUri;

        private Uri arrivedFromUri;

        private TransitionDirection currentAnimationType;

        private NavigationMode? currentNavigationMode;

        private bool isActive = true;

        private bool isAnimating;

        private bool isForwardNavigation = true;

        private bool isNavigating;

        private bool isOutroRequired;

        private bool loadingAndAnimatingIn;

        private Uri nextUri;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the AnimatePageBehavior class.
        /// </summary>
        public AnimatePageBehavior()
        {
            this.PageTransitions = new ObservableCollection<PageTransition>();
            this.DefaultTransition = TransitionStyle.Turnstile;
        }

        #endregion

        public FrameworkElement AnimationContext
        {
            get;
            set;
        }

        public TransitionStyle DefaultTransition
        {
            get;
            set;
        }

        public PhoneApplicationPage Page
        {
            get;
            set;
        }

        public ObservableCollection<PageTransition> PageTransitions
        {
            get;
            set;
        }

        protected virtual AnimatorBase GetAnimation(TransitionDirection direction, Uri fromAddress)
        {
            var animationOverride = this.PageTransitions.FirstOrDefault(
                    p => this.Matches(p.MatchUrl, fromAddress) || this.Matches(p.MatchUrl, this.nextUri));
            var style = this.DefaultTransition;
            if (animationOverride != null)
            {
                switch (direction)
                {
                    case TransitionDirection.ForwardOut:
                        if (this.Matches(animationOverride.MatchUrl, this.nextUri))
                        {
                            style = animationOverride.NavigatingTo;
                        }
                        break;
                    case TransitionDirection.ForwardIn:
                        if (this.Matches(animationOverride.MatchUrl, fromAddress))
                        {
                            style = animationOverride.NavigatedFrom;
                        }
                        break;
                    case TransitionDirection.BackwardIn:
                        if (this.Matches(animationOverride.MatchUrl, fromAddress))
                        {
                            style = animationOverride.ReturningFrom;
                        }
                        break;
                }
            }

            AnimatorBase animation = this.GetAnimation(direction, style);
            animation.RootElement = this.AnimationContext;
            return animation;
        }

        protected AnimatorBase GetAnimation(TransitionDirection animationType, TransitionStyle style)
        {
            switch (animationType)
            {
                case TransitionDirection.BackwardIn:
                    return this.GetBackwardInAnimator(style);
                case TransitionDirection.BackwardOut:
                    return this.GetBackwardOutAnimator(style);
                case TransitionDirection.ForwardIn:
                    return this.GetForwardInAnimator(style);
                case TransitionDirection.ForwardOut:
                    return this.GetForwardOutAnimator(style);
                default:
                    throw new ArgumentOutOfRangeException("animationType");
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (this.AssociatedObject != null)
            {
                this.Page = this.AssociatedObject;
                this.NavigatedTo();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.Page != null)
            {
                this.Page.Loaded -= this.PageLoaded;
                this.Page.BackKeyPress -= this.BackKeyPressed;
                if (this.Page.NavigationService != null)
                {
                    this.Page.NavigationService.Navigating -= this.NavigatingFrom;
                }
            }
        }

        protected void RunAnimation()
        {
            this.isAnimating = true;
            AnimatorBase animation = null;
            switch (this.currentAnimationType)
            {
                case TransitionDirection.ForwardIn:
                    animation = this.GetAnimation(this.currentAnimationType, fromUri);
                    break;
                case TransitionDirection.BackwardOut:
                    animation = this.GetAnimation(this.currentAnimationType, this.arrivedFromUri);
                    break;
                case TransitionDirection.BackwardIn:
                    animation = this.GetAnimation(this.currentAnimationType, this.arrivedFromUri);
                    break;
                default:
                    animation = this.GetAnimation(this.currentAnimationType, this.nextUri);
                    break;
            }

            this.Dispatcher.BeginInvoke(
                () =>
                    {
                        AnimatorBase transitionAnimation;
                        if (animation == null)
                        {
                            this.AnimationContext.Opacity = 1;
                            this.TransitionCompleted();
                        }
                        else
                        {
                            transitionAnimation = animation;
                            this.AnimationContext.Opacity = 1;
                            transitionAnimation.Begin(this.TransitionCompleted);
                        }
                    });
        }

        private void AnimationsComplete(TransitionDirection animationType)
        {
            // TODO: Determine if page has view model and if so load the page.
        }

        private void BackKeyPressed(object sender, CancelEventArgs e)
        {
            if (this.isNavigating)
            {
                e.Cancel = true;
                return;
            }

            if (!this.CanAnimate())
            {
                return;
            }

            if (this.isAnimating)
            {
                e.Cancel = true;
                return;
            }

            if (this.loadingAndAnimatingIn)
            {
                e.Cancel = true;
                return;
            }

            if (!this.Page.NavigationService.CanGoBack)
            {
                return;
            }

            if (!this.IsPopupOpen())
            {
                this.isNavigating = true;
                e.Cancel = true;
                this.isOutroRequired = false;
                this.currentAnimationType = TransitionDirection.BackwardOut;
                this.currentNavigationMode = NavigationMode.Back;
                this.RunAnimation();
            }
        }

        private bool CanAnimate()
        {
            return (this.isActive && !this.isNavigating && this.AnimationContext != null);
        }

        private AnimatorBase GetBackwardInAnimator(TransitionStyle style)
        {
            switch (style)
            {
                case TransitionStyle.Turnstile:
                    return new TurnstileBackwardInAnimator();
                case TransitionStyle.Slide:
                    return new SlideUpAnimator();
                default:
                    throw new ArgumentOutOfRangeException("style");
            }
        }

        private AnimatorBase GetBackwardOutAnimator(TransitionStyle style)
        {
            switch (style)
            {
                case TransitionStyle.Turnstile:
                    return new TurnstileBackwardOutAnimator();
                case TransitionStyle.Slide:
                    return new SlideDownAnimator();
                default:
                    throw new ArgumentOutOfRangeException("style");
            }
        }

        private AnimatorBase GetForwardInAnimator(TransitionStyle style)
        {
            switch (style)
            {
                case TransitionStyle.Turnstile:
                    return new TurnstileForwardInAnimator();
                case TransitionStyle.Slide:
                    return new SlideUpAnimator();
                default:
                    throw new ArgumentOutOfRangeException("style");
            }
        }

        private AnimatorBase GetForwardOutAnimator(TransitionStyle style)
        {
            switch (style)
            {
                case TransitionStyle.Turnstile:
                    return new TurnstileForwardOutAnimator();
                case TransitionStyle.Slide:
                    return new SlideDownAnimator();
                default:
                    throw new ArgumentOutOfRangeException("style");
            }
        }

        private bool IsPopupOpen()
        {
            return false;
        }

        private void LayoutUpdated()
        {
            if (this.isForwardNavigation)
            {
                this.currentAnimationType = TransitionDirection.ForwardIn;
                this.arrivedFromUri = fromUri != null ? new Uri(fromUri.OriginalString, UriKind.Relative) : null;
            }
            else
            {
                this.currentAnimationType = TransitionDirection.BackwardIn;
                this.isForwardNavigation = true;
            }

            if (this.CanAnimate())
            {
                this.RunAnimation();
            }
            else
            {
                if (this.AnimationContext != null)
                {
                    this.AnimationContext.Opacity = 1;
                }

                this.TransitionCompleted();
            }
        }

        /// <summary>
        /// Compares the address of a page being navigated to, with a declared address to match agains.
        /// </summary>
        /// <param name="targetAddress">The partial match string to check</param>
        /// <param name="address">The url to the page being navigated to to perform a partial match against</param>
        /// <returns>Returns true if the target address contains part of the specified address te</returns>
        private bool Matches(string targetAddress, Uri address)
        {
            if (address == null)
            {
                return (targetAddress == null);
            }

            if (targetAddress == null)
            {
                return false;
            }

            return address.OriginalString.IndexOf(targetAddress, StringComparison.OrdinalIgnoreCase) != -1;
        }

        private void NavigatedTo()
        {
            this.currentNavigationMode = null;
            if (this.nextUri != ExternalUri)
            {
                this.loadingAndAnimatingIn = true;
                this.Page.BackKeyPress += this.BackKeyPressed;
                this.Page.Loaded += this.PageLoaded;

                // Temporarily hide the page before it is loaded and prior to it's child animating in.
                this.Page.Opacity = 0;
            }

            this.isOutroRequired = true;
        }

        private void NavigatingFrom(object sender, NavigatingCancelEventArgs e)
        {
            if (this.isAnimating)
            {
                e.Cancel = true;
                return;
            }

            if (this.loadingAndAnimatingIn)
            {
                e.Cancel = true;
                return;
            }

            fromUri = this.Page.NavigationService.CurrentSource;

            if (this.isOutroRequired)
            {
                this.isOutroRequired = false;

                if (!this.CanAnimate())
                {
                    return;
                }

                if (this.isNavigating)
                {
                    e.Cancel = true;
                    return;
                }

                if (!this.Page.NavigationService.CanGoBack && e.NavigationMode == NavigationMode.Back)
                {
                    return;
                }

                if (this.IsPopupOpen())
                {
                    return;
                }

                e.Cancel = true;
                this.nextUri = e.Uri;

                switch (e.NavigationMode)
                {
                    case NavigationMode.New:
                        this.currentAnimationType = TransitionDirection.ForwardOut;
                        break;

                    case NavigationMode.Back:
                        this.currentAnimationType = TransitionDirection.BackwardOut;
                        break;

                    case NavigationMode.Forward:
                        this.currentAnimationType = TransitionDirection.ForwardOut;
                        break;
                }

                this.currentNavigationMode = e.NavigationMode;

                if (e.Uri != ExternalUri)
                {
                    this.RunAnimation();
                }
            }
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (this.Page.NavigationService == null)
            {
                return;
            }

            this.Page.NavigationService.Navigating -= this.NavigatingFrom;
            this.Page.NavigationService.Navigating += this.NavigatingFrom;

            this.AnimationContext = this.AssociatedObject.FindName("LayoutRoot") as FrameworkElement;
            if (this.AnimationContext != null)
            {
                this.AnimationContext.Opacity = 0;
            }

            this.Page.Opacity = 1;
            this.LayoutUpdated();
        }

        /// <summary>
        /// Clears and resets values as navigation has completed, page may actually persist in memory regardless of this.
        /// </summary>
        private void PageUnloaded()
        {
            this.currentNavigationMode = null;
            if (this.nextUri != null)
            {
                this.arrivedFromUri = this.nextUri;
            }

            this.nextUri = null;
            this.isForwardNavigation = false;
            this.Page.Opacity = 0;
            if (this.AnimationContext != null)
            {
                this.AnimationContext.Opacity = 0;
            }
            //this.isNavigating = false;
            //this.isOutroRequired = true;
        }

        private void TransitionCompleted()
        {
            this.isAnimating = false;
            this.loadingAndAnimatingIn = false;

            if (!this.isNavigating && this.currentAnimationType == TransitionDirection.BackwardOut)
            {
                this.PageUnloaded();
            }

            try
            {
                this.Dispatcher.BeginInvoke(
                    () =>
                        {
                            this.isNavigating = false;
                            switch (this.currentNavigationMode)
                            {
                                case NavigationMode.Forward:
                                    this.PageUnloaded();
                                    Application.Current.GoForward();
                                    break;

                                case NavigationMode.Back:
                                    this.PageUnloaded();
                                    Application.Current.GoBack();
                                    break;

                                case NavigationMode.New:
                                    var url = this.nextUri;
                                    this.PageUnloaded();
                                    Application.Current.Navigate(url);
                                    break;
                            }
                        });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnTransitionAnimationCompleted Exception on {0}: {1}", this, ex);
            }

            this.AnimationsComplete(this.currentAnimationType);
        }
    }
}