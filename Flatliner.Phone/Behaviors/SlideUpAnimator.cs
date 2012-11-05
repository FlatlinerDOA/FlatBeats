namespace Flatliner.Phone.Behaviors
{
    using System.Windows.Markup;
    using System.Windows.Media.Animation;

    public class SlideUpAnimator : SlideAnimator
    {
        private static Storyboard _storyboard;

        public SlideUpAnimator()
            : base()
        {
            if (_storyboard == null)
                _storyboard = XamlReader.Load(Storyboards.SlideUpFadeInStoryboard) as Storyboard;

            Storyboard = _storyboard;
        }
    }
}