namespace Flatliner.Phone.Behaviors
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Flatliner.Phone.Extensions;

    public class TurnstileAnimator : AnimatorBase
    {
        public override void Begin(Action completionAction)
        {
            if (this.PrepareElement(RootElement))
            {
                (RootElement.Projection as PlaneProjection).CenterOfRotationX = 0;
                Storyboard.Stop();
                base.SetTarget(RootElement);
            }

            base.Begin(completionAction);
        }

        private bool PrepareElement(UIElement element)
        {
            if (VisualTreeExtensions.GetPlaneProjection(element, true) == null)
            {
                return false;
            }
            return true;
        }
    }
}