namespace Flatliner.Phone.Behaviors
{
    using System.Windows.Markup;
    using System.Windows.Media.Animation;

    public class TurnstileBackwardInAnimator : TurnstileAnimator
    {
        private static Storyboard _storyboard;

        public TurnstileBackwardInAnimator()
            : base()
        {
            if (_storyboard == null)
                _storyboard = XamlReader.Load(Storyboards.TurnstileBackwardInStoryboard) as Storyboard;

            Storyboard = _storyboard;
        }
    }
}