namespace Flatliner.Phone.Behaviors
{
    using System.Windows.Markup;
    using System.Windows.Media.Animation;

    public class TurnstileBackwardOutAnimator : TurnstileAnimator
    {
        private static Storyboard _storyboard;

        public TurnstileBackwardOutAnimator()
            : base()
        {
            if (_storyboard == null)
                _storyboard = XamlReader.Load(Storyboards.TurnstileBackwardOutStoryboard) as Storyboard;

            Storyboard = _storyboard;
        }
    }
}