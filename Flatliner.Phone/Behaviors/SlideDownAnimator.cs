namespace Flatliner.Phone.Behaviors
{
    using System.Windows.Markup;
    using System.Windows.Media.Animation;

    public class SlideDownAnimator : SlideAnimator
    {
        private static Storyboard storyboard;

        public SlideDownAnimator()
            : base()
        {
            if (storyboard == null)
                storyboard = XamlReader.Load(Storyboards.SlideDownFadeOutStoryboard) as Storyboard;

            Storyboard = storyboard;
        }
    }
}