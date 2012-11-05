namespace Flatliner.Phone.Behaviors
{
    public enum TransitionStyle
    {
        /// <summary>
        /// No transitions.
        /// </summary>
        None,

        /// <summary>
        /// Door swing animation is a fairly heavyweight transition useful for application start pages etc.
        /// </summary>
        Turnstile,
        
        /// <summary>
        /// Useful for signifying a navigation to a dead end. 
        /// For example when creating a new item, the user assumes the only way to go from there is to return to where they came from (get-in get-out animation).
        /// Depending on the direction the page will slide up (navigating to) or slide down (navigating from)
        /// </summary>
        Slide,

        // TODO: Continuum and ProgressiveTurnstile
    }
}
