namespace Flatliner.Phone.Behaviors
{
    using System.Windows.Markup;
    using System.Windows.Media.Animation;

    public class DefaultPageAnimator : TurnstileAnimator
    {
        private static Storyboard _storyboard;

        public DefaultPageAnimator()
            : base()
        {
            if (_storyboard == null)
                _storyboard = XamlReader.Load(Storyboards.DefaultStoryboard) as Storyboard;

            Storyboard = XamlReader.Load(Storyboards.DefaultStoryboard) as Storyboard;
        }
    }
}