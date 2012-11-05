namespace Flatliner.Phone.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media.Animation;

    public abstract class AnimatorBase
    {
        private Action oneTimeAction;

        public Storyboard Storyboard { get; set; }

        // Methods
        protected AnimatorBase()
        {

        }

        private void OnCompleted(object sender, EventArgs e)
        {
            Storyboard.Completed -= this.OnCompleted;
            Action action = oneTimeAction;
            if (action != null)
            {
                oneTimeAction = null;
                action();
            }
        }

        public virtual void Begin(Action completionAction)
        {
            Storyboard.Stop();
            Storyboard.Begin();
            Storyboard.SeekAlignedToLastTick(TimeSpan.Zero);
            Storyboard.Completed += new EventHandler(OnCompleted);
            oneTimeAction = completionAction;
        }

        public void SetTargets(Dictionary<string, FrameworkElement> targets, Storyboard sb)
        {
            foreach (var kvp in targets)
            {
                KeyValuePair<string, FrameworkElement> kvp1 = kvp;
                var timelines = sb.Children.Where(t => Storyboard.GetTargetName(t) == kvp1.Key);
                foreach (Timeline t in timelines)
                {
                    Storyboard.SetTarget(t, kvp.Value);
                }
            }
        }

        public void SetTargets(Dictionary<string, FrameworkElement> targets)
        {
            SetTargets(targets, Storyboard);
        }

        public void SetTarget(FrameworkElement target)
        {
            foreach (Timeline t in Storyboard.Children)
            {
                Storyboard.SetTarget(t, target);
            }
        }

        public FrameworkElement RootElement { get; set; }
    }
}