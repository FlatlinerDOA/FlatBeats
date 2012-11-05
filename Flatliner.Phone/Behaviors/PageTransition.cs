namespace Flatliner.Phone.Behaviors
{
    using System;

    /// <summary>
    /// Defines a transition when navigating specifically to a page with the specified partial url.
    /// </summary>
    public class PageTransition
    {
        /// <summary>
        /// Initializes a new instance of the PageTransition class.
        /// </summary>
        public PageTransition()
        {
            this.NavigatedFrom = TransitionStyle.Turnstile;
            this.NavigatingTo = TransitionStyle.Turnstile;
            this.ReturningFrom = TransitionStyle.Turnstile;
        }

        /// <summary>
        /// Gets or sets the partial url to compare against when navigating, will match against anything including the querystring
        /// </summary>
        public string MatchUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the animation to use when this page is the source.
        /// </summary>
        public TransitionStyle NavigatedFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the animation the current page should perform when navigating to the specified page.
        /// </summary>
        public TransitionStyle NavigatingTo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the animation to use when returning from the specified page (usually the inverse of.
        /// </summary>
        public TransitionStyle ReturningFrom
        {
            get;
            set;
        }
    }
}
