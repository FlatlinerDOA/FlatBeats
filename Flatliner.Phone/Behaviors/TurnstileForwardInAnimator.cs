namespace Flatliner.Phone.Behaviors
{
    using System.Windows.Markup;
    using System.Windows.Media.Animation;

    public class TurnstileForwardInAnimator : TurnstileAnimator
    {
        private static Storyboard storyboard;

        public TurnstileForwardInAnimator()
            : base()
        {
            if (storyboard == null)
                storyboard = XamlReader.Load(Storyboards.TurnstileForwardInStoryboard) as Storyboard;

            Storyboard = storyboard;
        }
    }
}