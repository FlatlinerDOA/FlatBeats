namespace Flatliner.Phone.Behaviors
{
    using System.Windows.Markup;
    using System.Windows.Media.Animation;

    public class TurnstileForwardOutAnimator : TurnstileAnimator
    {
        private static Storyboard storyboard;

        public TurnstileForwardOutAnimator()
            : base()
        {
            if (storyboard == null)
                storyboard = XamlReader.Load(Storyboards.TurnstileForwardOutStoryboard) as Storyboard;

            Storyboard = storyboard;
        }
    }
}