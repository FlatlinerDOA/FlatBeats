namespace Flatliner.Phone.Behaviors
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Flatliner.Phone.Extensions;

    public class SlideAnimator : AnimatorBase
    {
        public override void Begin(Action completionAction)
        {
            if (this.PrepareElement(RootElement))
            {
                Storyboard.Stop();
                base.SetTarget(RootElement);
            }

            base.Begin(completionAction);
        }

        private bool PrepareElement(UIElement element)
        {
            element.GetTransform<CompositeTransform>(TransformCreationMode.CreateOrAddAndIgnoreMatrix);

            return true;
        }
    }
}
